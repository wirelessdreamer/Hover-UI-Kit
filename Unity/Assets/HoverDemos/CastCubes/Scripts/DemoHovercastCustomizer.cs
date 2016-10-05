﻿using System;
using Hover.Core.Cursors;
using Hover.Core.Items.Types;
using Hover.Core.Renderers;
using Hover.Core.Renderers.CanvasElements;
using Hover.Core.Renderers.Items.Buttons;
using Hover.Core.Renderers.Items.Sliders;
using Hover.Core.Utils;
using Hover.InterfaceModules.Cast;
using Hover.RendererModules.Alpha;
using UnityEngine;
using UnityEngine.UI;

namespace HoverDemos.CastCubes {

	/*================================================================================================*/
	[RequireComponent(typeof(HovercastInterface))]
	public class DemoHovercastCustomizer : MonoBehaviour {

		[Serializable]
		public class Theme {
			public Color TextColor = new Color(1, 1, 1);
			public Color IconColor = new Color(1, 1, 1);
			public Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
			public Color HighlightColor = new Color(0.25f, 0.25f, 0.25f, 0.43f);
			public Color SelectionColor = new Color(0.5f, 0.5f, 0.5f, 1);
			public Color EdgeColor = new Color(0.5f, 0.5f, 0.5f, 1);
			public Color FlashColor = new Color(1, 1, 1, 1);
			public Color SliderTrackColor = new Color(0.1f, 0.1f, 0.1f, 0.25f);
			public Color SliderFillColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
			public Color SliderTickColor = new Color(1, 1, 1, 0.25f);
		}

		public Theme DarkTheme = new Theme();

		public Theme LightTheme = new Theme {
			TextColor = new Color(0, 0, 0),
			IconColor = new Color(0, 0, 0),
			BackgroundColor = new Color(1, 1, 1, 0.25f),
			HighlightColor = new Color(1, 1, 1, 0.25f),
			SelectionColor = new Color(1, 1, 1, 1),
			EdgeColor = new Color(1, 1, 1, 1),
			FlashColor = new Color(1, 1, 1, 1),
			SliderTrackColor = new Color(1, 1, 1, 0.15f),
			SliderFillColor = new Color(1, 1, 1, 0.5f),
			SliderTickColor = new Color(0, 0, 0, 0.5f),
		};

		public Theme ColorTheme = new Theme {
			TextColor = new Color(1, 1, 0.7f),
			IconColor = new Color(1, 1, 0.7f),
			BackgroundColor = new Color(0.05f, 0.25f, 0.45f, 0.5f),
			HighlightColor = new Color(0.1f, 0.5f, 0.9f),
			SelectionColor = new Color(0.1f, 0.9f, 0.2f),
			EdgeColor = new Color(0.1f, 0.9f, 0.2f),
			FlashColor = new Color(1, 1, 0.7f, 1),
			SliderTrackColor = new Color(0.1f, 0.5f, 0.9f, 0.5f),
			SliderFillColor = new Color(0.1f, 0.9f, 0.2f, 0.5f),
			SliderTickColor = new Color(1, 1, 0.7f, 0.2f),
		};

		private Theme vCurrTheme;
		private float vCurrBgAlpha;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public DemoHovercastCustomizer() {
			vCurrTheme = DarkTheme;
			vCurrBgAlpha = DarkTheme.BackgroundColor.a;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void SetDarkTheme() {
			SetTheme(DarkTheme);
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SetLightTheme() {
			SetTheme(LightTheme);
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SetColorTheme() {
			SetTheme(ColorTheme);
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SetFontSize(IItemDataSelectable<float> pItemData) {
			HoverCanvas[] canvases = GetComponentsInChildren<HoverCanvas>(true);
			int size = Mathf.RoundToInt(((IItemDataSlider)pItemData).SnappedRangeValue);

			foreach ( HoverCanvas canvas in canvases ) {
				canvas.Label.TextComponent.fontSize = size;
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SetBgAlpha(IItemDataSelectable<float> pItemData) {
			float bgAlpha = ((IItemDataSlider)pItemData).SnappedRangeValue;

			if ( Math.Abs(bgAlpha-vCurrBgAlpha) < 2/255f ) {
				return;
			}

			vCurrBgAlpha = bgAlpha;
			SetTheme(vCurrTheme);
		}

		/*--------------------------------------------------------------------------------------------*/
		public void SwitchHands() {
			HoverCursorFollower follow = GetComponent<HoverCursorFollower>();
			HoverCursorDataProvider curProv = follow.CursorDataProvider;
			ICursorDataForInput palmR = curProv.GetCursorDataForInput(CursorType.RightPalm);
			ICursorDataForInput palmL = curProv.GetCursorDataForInput(CursorType.LeftPalm);
			ICursorDataForInput indxR = curProv.GetCursorDataForInput(CursorType.RightIndex);
			ICursorDataForInput indxL = curProv.GetCursorDataForInput(CursorType.LeftIndex);
			HovercastMirrorSwitcher mirror = GetComponent<HovercastMirrorSwitcher>();
			bool isMirror = !mirror.UseMirrorLayout;

			if ( isMirror ) {
				mirror.UseMirrorLayout = true;
				follow.CursorType = CursorType.RightPalm;

				palmR.SetCapability(CursorCapabilityType.TransformOnly);
				indxL.SetCapability(CursorCapabilityType.Full);
				palmL.SetCapability(CursorCapabilityType.None);
				indxR.SetCapability(CursorCapabilityType.None);

				palmR.gameObject.SetActive(true);
				indxL.gameObject.SetActive(true);
				palmL.gameObject.SetActive(false);
				indxR.gameObject.SetActive(false);
			}
			else {
				mirror.UseMirrorLayout = false;
				follow.CursorType = CursorType.LeftPalm;

				palmR.SetCapability(CursorCapabilityType.None);
				indxL.SetCapability(CursorCapabilityType.None);
				palmL.SetCapability(CursorCapabilityType.TransformOnly);
				indxR.SetCapability(CursorCapabilityType.Full);

				palmR.gameObject.SetActive(false);
				indxL.gameObject.SetActive(false);
				palmL.gameObject.SetActive(true);
				indxR.gameObject.SetActive(true);
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void SetTheme(Theme pTheme) {
			HoverFillButton[] buttonFills = GetComponentsInChildren<HoverFillButton>(true);
			HoverFillSlider[] sliderFills = GetComponentsInChildren<HoverFillSlider>(true);
			HoverCanvas[] canvases = GetComponentsInChildren<HoverCanvas>(true);
			Color fadeBgCol = DisplayUtil.FadeColor(pTheme.BackgroundColor, vCurrBgAlpha);
			Color fadeTrackCol = DisplayUtil.FadeColor(pTheme.SliderTrackColor, vCurrBgAlpha);

			foreach ( HoverFillButton fill in buttonFills ) {
				SetMeshColors(fill.Background, fadeBgCol, pTheme.FlashColor);
				SetMeshColors(fill.Highlight, pTheme.HighlightColor, pTheme.FlashColor);
				SetMeshColors(fill.Selection, pTheme.SelectionColor, pTheme.FlashColor);
				SetMeshColors(fill.Edge, pTheme.EdgeColor, pTheme.FlashColor);
			}

			foreach ( HoverFillSlider fill in sliderFills ) {
				SetMeshColors(fill.SegmentA, fadeTrackCol, pTheme.FlashColor, pTheme.SliderFillColor);
				SetMeshColors(fill.SegmentB, fadeTrackCol, pTheme.FlashColor, pTheme.SliderFillColor);
				SetMeshColors(fill.SegmentC, fadeTrackCol, pTheme.FlashColor, pTheme.SliderFillColor);
				SetMeshColors(fill.SegmentD, fadeTrackCol, pTheme.FlashColor, pTheme.SliderFillColor);

				foreach ( HoverMesh tickMesh in fill.Ticks ) {
					SetMeshColors(tickMesh, pTheme.SliderTickColor, pTheme.FlashColor);
				}
			}

			foreach ( HoverCanvas canvas in canvases ) {
				canvas.Label.TextComponent.color = pTheme.TextColor;
				//canvas.IconInner.ImageComponent.color = pTheme.IconColor;
				//canvas.IconOuter.ImageComponent.color = pTheme.IconColor;

				RawImage[] iconImages = canvas.GetComponentsInChildren<RawImage>();

				foreach ( RawImage iconImage in iconImages ) {
					iconImage.color = pTheme.IconColor;
				}
			}

			vCurrTheme = pTheme;
		}

		/*--------------------------------------------------------------------------------------------*/
		private void SetMeshColors(HoverMesh pMesh, Color pStandardColor, Color pFlashColor) {
			SetMeshColors(pMesh, pStandardColor, pFlashColor, pStandardColor);
		}

		/*--------------------------------------------------------------------------------------------*/
		private void SetMeshColors(HoverMesh pMesh, Color pStandardColor, 
																Color pFlashColor, Color pFillColor) {
			HoverAlphaMeshUpdater alphaUp = pMesh.GetComponent<HoverAlphaMeshUpdater>();
			alphaUp.StandardColor = pStandardColor;
			alphaUp.FlashColor = pFlashColor;
			alphaUp.SliderFillColor = pFillColor;
		}

	}

}