﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Registers a custom input to the wave shape. Attach this GameObjects that you want to render into the displacmeent textures to affect ocean shape.
    /// </summary>
    [AddComponentMenu(MENU_PREFIX + "Animated Waves Input")]
    [HelpURL(Internal.Constants.HELP_URL_BASE_USER + "waves.html" + Internal.Constants.HELP_URL_RP + "#wave-placement")]
    public class RegisterAnimWavesInput : RegisterLodDataInputWithSplineSupport<LodDataMgrAnimWaves>, LodDataMgrAnimWaves.IShapeUpdatable
    {
        /// <summary>
        /// The version of this asset. Can be used to migrate across versions. This value should
        /// only be changed when the editor upgrades the version.
        /// </summary>
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _version = 0;
#pragma warning restore 414

        public override bool Enabled => true;

        [Header("Anim Waves Input Settings")]

        [Tooltip("Whether to filter this input by wavelength. If disabled it will render to all LODs.")]
        [SerializeField]
        bool _filterByWavelength;

        [Tooltip("Which octave to render into, for example set this to 2 to use render into the 2m-4m octave. These refer to the same octaves as the wave spectrum editor. Set this value to 0 to render into all LODs after Dynamic Waves.")]
        [Predicated(nameof(_filterByWavelength))]
        [SerializeField, DecoratedField]
        float _octaveWavelength = 0f;
        public override float Wavelength => _filterByWavelength ? _octaveWavelength : _renderAfterDynamicWaves ? 0 : -1;

        [Tooltip("Render to all LODs before the combine step where Dynamic Waves are added to the Animated Waves. Useful for scaling waves etc without affecting ripples and wakes.")]
        [Predicated(nameof(_filterByWavelength), inverted: true)]
        [SerializeField, DecoratedField]
        bool _renderAfterDynamicWaves = true;

        public readonly static Color s_gizmoColor = new Color(0f, 1f, 0f, 0.5f);
        protected override Color GizmoColor => s_gizmoColor;

        protected override string ShaderPrefix => "Crest/Inputs/Animated Waves";

        protected override string SplineShaderName => "Crest/Inputs/Animated Waves/Set Base Water Height Using Geometry";
        protected override Vector2 DefaultCustomData => Vector2.zero;

        [SerializeField, Tooltip(k_displacementCorrectionTooltip)]
        bool _followHorizontalMotion = true;
        protected override bool FollowHorizontalMotion => base.FollowHorizontalMotion || _followHorizontalMotion;

        [SerializeField, Tooltip("Inform ocean how much this input will displace the ocean surface vertically. This is used to set bounding box heights for the ocean tiles.")]
        float _maxDisplacementVertical = 0f;
        [SerializeField, Tooltip("Inform ocean how much this input will displace the ocean surface horizontally. This is used to set bounding box widths for the ocean tiles.")]
        float _maxDisplacementHorizontal = 0f;

        [SerializeField, Tooltip("Use the bounding box of an attached renderer component to determine the max vertical displacement.")]
        [Predicated(typeof(MeshRenderer)), DecoratedField]
        bool _reportRendererBoundsToOceanSystem = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            LodDataMgrAnimWaves.RegisterUpdatable(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            LodDataMgrAnimWaves.DeregisterUpdatable(this);
        }

        public void CrestUpdate(UnityEngine.Rendering.CommandBuffer buf)
        {
            if (OceanRenderer.Instance == null)
            {
                return;
            }

            var maxDispVert = _maxDisplacementVertical;

            // let ocean system know how far from the sea level this shape may displace the surface
            if (_reportRendererBoundsToOceanSystem)
            {
                var minY = _renderer.bounds.min.y;
                var maxY = _renderer.bounds.max.y;
                var seaLevel = OceanRenderer.Instance.SeaLevel;
                maxDispVert = Mathf.Max(maxDispVert, Mathf.Abs(seaLevel - minY), Mathf.Abs(seaLevel - maxY));
            }

            if (_maxDisplacementHorizontal > 0f || maxDispVert > 0f)
            {
                OceanRenderer.Instance.ReportMaxDisplacementFromShape(_maxDisplacementHorizontal, maxDispVert, 0f);
            }
        }
    }
}
