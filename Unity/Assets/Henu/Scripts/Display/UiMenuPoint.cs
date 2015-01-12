﻿using System;
using Henu.Navigation;
using Henu.State;
using UnityEngine;

namespace Henu.Display {

	/*================================================================================================*/
	public class UiMenuPoint : MonoBehaviour {

		public static float ItemChangeMilliseconds = 1000;
		public static float ItemChangeDistance = 0.08f;

		private ArcState vHand;
		private ArcSegmentState vPoint;
		private Renderers vRenderers;

		private GameObject vPrevRendererObj;
		private GameObject vCurrRendererObj;
		private IUiMenuPointRenderer vPrevRenderer;
		private IUiMenuPointRenderer vCurrRenderer;

		private int vRendererCount;
		private DateTime? vChangeTime;
		private int vChangeDir;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void Build(ArcState pHand, ArcSegmentState pPoint, Renderers pRenderers) {
			vHand = pHand;
			vPoint = pPoint;
			vRenderers = pRenderers;

			//vPoint.OnNavItemChange += HandleNavItemChange;
			HandleNavItemChange(0);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public void Update() {
			/*if ( !vPoint.IsActive ) {
				return;
			}

			Transform tx = gameObject.transform;
			tx.localPosition = vPoint.Position;
			tx.localRotation = vPoint.Rotation;

			if ( !vHand.IsLeft ) {
				tx.localRotation *= Quaternion.FromToRotation(Vector3.left, Vector3.right);
			}*/

			UpdateItemChangeAnim();
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public bool IsActive() {
			return false; //(vPoint != null && vPoint.IsActive);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void HandleNavItemChange(int pDirection) {
			DestroyPrevRenderer();
			vPrevRendererObj = vCurrRendererObj;
			vPrevRenderer = vCurrRenderer;

			if ( vPoint.NavItem == null ) {
				vCurrRendererObj = null;
				vCurrRenderer = null;
			}
			else {
				BuildCurrRenderer();
			}

			vChangeTime = DateTime.UtcNow;
			vChangeDir = pDirection;
			UpdateItemChangeAnim();
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private void DestroyPrevRenderer() {
			if ( vPrevRendererObj == null ) {
				return;
			}

			vPrevRendererObj.SetActive(false);
			Destroy(vPrevRendererObj);

			vPrevRendererObj = null;
			vPrevRenderer = null;
		}

		/*--------------------------------------------------------------------------------------------*/
		private void BuildCurrRenderer() {
			vCurrRendererObj = new GameObject("Renderer"+vRendererCount);
			vRendererCount++;

			Type rendererType;

			switch ( vPoint.NavItem.Type ) {
				case NavItem.ItemType.Parent:
					rendererType = vRenderers.PointParent;
					break;

				case NavItem.ItemType.Checkbox:
					rendererType = vRenderers.PointCheckbox;
					break;

				case NavItem.ItemType.Radio:
					rendererType = vRenderers.PointRadio;
					break;

				default:
					rendererType = vRenderers.PointSelection;
					break;
			}

			vCurrRenderer = (IUiMenuPointRenderer)vCurrRendererObj.AddComponent(rendererType);
			vCurrRenderer.Build(vHand, vPoint);
			vCurrRenderer.Update();

			vCurrRendererObj.transform.parent = gameObject.transform;
			vCurrRendererObj.transform.localPosition = Vector3.zero;
			vCurrRendererObj.transform.localRotation = Quaternion.identity;
			vCurrRendererObj.transform.localScale = Vector3.one;
		}

		/*--------------------------------------------------------------------------------------------*/
		private void UpdateItemChangeAnim() {
			if ( vChangeTime == null ) {
				return;
			}

			float ms = (float)(DateTime.UtcNow-(DateTime)vChangeTime).TotalMilliseconds;
			float prog = Math.Min(1, ms/ItemChangeMilliseconds);
			float push = 1-(float)Math.Pow(1-prog, 3);
			float dist = -ItemChangeDistance*vChangeDir;

			if ( vPrevRenderer != null ) {
				vPrevRenderer.HandleChangeAnimation(false, vChangeDir, prog);
				vPrevRendererObj.transform.localPosition = new Vector3(0, 0, -dist*push);
			}

			if ( vCurrRenderer != null ) {
				vCurrRenderer.HandleChangeAnimation(true, vChangeDir, prog);
				vCurrRendererObj.transform.localPosition = new Vector3(0, 0, dist*(1-push));
			}

			if ( prog >= 1 ) {
				vChangeTime = null;
				DestroyPrevRenderer();
			}

			vPoint.SetIsAnimating(vChangeTime != null);
		}

	}

}