﻿using System;
using System.Collections.Generic;
using Hover.Core.Cursors;
using Hover.Core.Items.Managers;
using Hover.Core.Items.Types;
using Hover.Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hover.Core.Items {

	/*================================================================================================*/
	[ExecuteInEditMode]
	public class HoverItemHighlightState : TreeUpdateableBehavior {

		[Serializable]
		public struct Highlight {
			public bool IsNearestAcrossAllItems;
			public ICursorData Cursor;
			public Vector3 NearestWorldPos;
			public float Distance;
			public float Progress;
		}

		public bool IsHighlightPrevented { get; private set; }
		public Highlight? NearestHighlight { get; private set; }
		public List<Highlight> Highlights { get; private set; }
		public bool IsNearestAcrossAllItemsForAnyCursor { get; private set; }
		public bool HasCursorInProximity { get; private set; }

		[FormerlySerializedAs("CursorDataProvider")]
		public HoverCursorDataProvider _CursorDataProvider;

		[FormerlySerializedAs("ProximityProvider")]
		public HoverItemRendererUpdater _ProximityProvider;

		[FormerlySerializedAs("InteractionSettings")]
		public HoverInteractionSettings _InteractionSettings;

		private readonly HashSet<string> vPreventHighlightMap;
		private readonly HashSet<CursorType> vIsNearestForCursorTypeMap;

		/*private bool _IsHighPrevented;
		private Highlight? _NearestHighlight;
		private List<Highlight> _Highlights;
		private bool _IsNearestAcrossAllItemsForAnyCursor;*/


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public HoverItemHighlightState() {
			Highlights = new List<Highlight>();
			vPreventHighlightMap = new HashSet<string>();
			vIsNearestForCursorTypeMap = new HashSet<CursorType>(new CursorTypeComparer());
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public HoverCursorDataProvider CursorDataProvider {
			get => _CursorDataProvider;
			set => this.UpdateValueWithTreeMessage(ref _CursorDataProvider, value, "CursorDataProv");
		}

		/*--------------------------------------------------------------------------------------------*/
		public HoverItemRendererUpdater ProximityProvider {
			get => _ProximityProvider;
			set => this.UpdateValueWithTreeMessage(ref _ProximityProvider, value, "ProximityProv");
		}

		/*--------------------------------------------------------------------------------------------*/
		public HoverInteractionSettings InteractionSettings {
			get => _InteractionSettings;
			set => this.UpdateValueWithTreeMessage(ref _InteractionSettings, value, "InteractionSett");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------* /
		public bool IsHighlightPrevented {
			get => _IsHighPrevented;
			private set => this.UpdateValueWithTreeMessage(ref _IsHighPrevented, value, "IsHighPrev");
		}

		/*--------------------------------------------------------------------------------------------* /
		public bool IsNearestAcrossAllItemsForAnyCursor {
			get => _IsNearestAcrossAllItemsForAnyCursor;
			private set => this.UpdateValueWithTreeMessage(
				ref _IsNearestAcrossAllItemsForAnyCursor, value, "IsNearest");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void Awake() {
			if ( CursorDataProvider == null ) {
				CursorDataProvider = HoverCursorDataProvider.Instance;
			}

			if ( ProximityProvider == null ) {
				ProximityProvider = GetComponent<HoverItemRendererUpdater>();
			}

			if ( InteractionSettings == null ) {
				InteractionSettings = (GetComponent<HoverInteractionSettings>() ??
					HoverItemsManager.Instance?.GetComponent<HoverInteractionSettings>());
			}

			if ( CursorDataProvider == null ) {
				Debug.LogWarning("Could not find 'CursorDataProvider'.");
			}

			if ( ProximityProvider == null ) {
				//TODO: show warning elsewhere? the renderer is typically added *after* this
				//Debug.LogWarning("Could not find 'ProximityProvider'.");
			}

			if ( InteractionSettings == null ) {
				Debug.LogWarning("Could not find 'InteractionSettings'.");
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public override void TreeUpdate() {
			//do nothing...
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public Highlight? GetHighlight(CursorType pType) {
			for ( int i = 0 ; i < Highlights.Count ; i++ ) {
				Highlight high = Highlights[i];

				if ( high.Cursor.Type == pType ) {
					return high;
				}
			}

			return null;
		}

		/*--------------------------------------------------------------------------------------------*/
		public float MaxHighlightProgress {
			get {
				IItemDataSelectable selData = (GetComponent<HoverItemData>() as IItemDataSelectable);

				if ( selData != null && selData.IsStickySelected ) {
					return 1;
				}

				return (NearestHighlight == null ? 0 : NearestHighlight.Value.Progress);
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public void ResetAllNearestStates() {
			if ( vIsNearestForCursorTypeMap.Count > 0 ) {
				vIsNearestForCursorTypeMap.Clear();
				TreeUpdater.SendTreeUpdatableChanged(this, "ResetState");
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SetNearestAcrossAllItemsForCursor(CursorType pType) {
			vIsNearestForCursorTypeMap.Add(pType);
			TreeUpdater.SendTreeUpdatableChanged(this, "SetNearest");
		}

		/*--------------------------------------------------------------------------------------------*/
		public void UpdateViaManager() {
			bool hadCursorInProx = HasCursorInProximity;

			HasCursorInProximity = false;
			Highlights.Clear();

			NearestHighlight = null;
			UpdateIsHighlightPrevented();
			AddLatestHighlightsAndFindNearest();

			if ( HasCursorInProximity ) { //always update when in proximity
				TreeUpdater.SendTreeUpdatableChanged(this, "ProximityActive");
			}
			else if ( hadCursorInProx ) {
				TreeUpdater.SendTreeUpdatableChanged(this, "ProximityLost");
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void PreventHighlightViaDisplay(string pName, bool pPrevent) {
			bool didChange;

			if ( pPrevent ) {
				didChange = vPreventHighlightMap.Add(pName);
			}
			else {
				didChange = vPreventHighlightMap.Remove(pName);
			}

			if ( didChange ) {
				TreeUpdater.SendTreeUpdatableChanged(this, "PreventHighlight");
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public bool IsHighlightPreventedViaAnyDisplay() {
			return (vPreventHighlightMap.Count > 0);
		}

		/*--------------------------------------------------------------------------------------------*/
		public bool IsHighlightPreventedViaDisplay(string pName) {
			return vPreventHighlightMap.Contains(pName);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateIsHighlightPrevented() {
			HoverItem hoverItem = GetComponent<HoverItem>();
			HoverItemData itemData = GetComponent<HoverItem>().Data;
			IItemDataSelectable selData = (itemData as IItemDataSelectable);
			bool prevIsPrevented = IsHighlightPrevented;

			IsHighlightPrevented = (
				selData == null ||
				!hoverItem.gameObject.activeInHierarchy ||
				IsHighlightPreventedViaAnyDisplay()
			);

			if ( IsHighlightPrevented != prevIsPrevented ) {
				TreeUpdater.SendTreeUpdatableChanged(this, "IsHighlightPrevented");
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		private void AddLatestHighlightsAndFindNearest() {
			if ( IsHighlightPrevented || ProximityProvider == null || 
					CursorDataProvider == null || InteractionSettings == null ) {
				return;
			}

			float minDist = float.MaxValue;
			List<ICursorData> cursors = CursorDataProvider.Cursors;
			int cursorCount = cursors.Count;
			
			for ( int i = 0 ; i < cursorCount ; i++ ) {
				ICursorData cursor = cursors[i];

				if ( !cursor.CanCauseSelections ) {
					continue;
				}

				Highlight high = CalculateHighlight(cursor);
				high.IsNearestAcrossAllItems = vIsNearestForCursorTypeMap.Contains(cursor.Type);
				Highlights.Add(high);

				if ( high.Distance >= minDist ) {
					continue;
				}

				minDist = high.Distance;
				NearestHighlight = high;
			}

			IsNearestAcrossAllItemsForAnyCursor = (vIsNearestForCursorTypeMap.Count > 0);
		}

		/*--------------------------------------------------------------------------------------------*/
		private Highlight CalculateHighlight(ICursorData pCursor) {
			var high = new Highlight();
			high.Cursor = pCursor;
			
			if ( !Application.isPlaying ) {
				return high;
			}

			Vector3 cursorWorldPos = (pCursor.BestRaycastResult == null ?
				pCursor.WorldPosition : pCursor.BestRaycastResult.Value.WorldPosition);

			high.NearestWorldPos = ProximityProvider.GetNearestWorldPosition(cursorWorldPos);
			high.Distance = (cursorWorldPos-high.NearestWorldPos).magnitude;
			high.Progress = Mathf.InverseLerp(InteractionSettings.HighlightDistanceMax,
				InteractionSettings.HighlightDistanceMin, high.Distance);

			if ( high.Progress > 0 ) {
				HasCursorInProximity = true;
			}

			return high;
		}

	}

}