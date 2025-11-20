using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMColorFast))]
[CanEditMultipleObjects]
public class FMColorFast_Editor : Editor
{
    private Texture2D logo;
    private FMColorFast FC;

    private bool ShowPreset = false;
    private bool ShowBasicSettings = false;
    private bool ShowGrainSettings = false;
    private bool ShowVignetteSettings = false;
    private bool ShowScanlineSettings = false;

    SerializedProperty LutModeProp;
    SerializedProperty LutContributionProp;
    SerializedProperty LookupTextureProp;

    SerializedProperty PixelSizeProp;
    SerializedProperty CelCutsProp;

    SerializedProperty DebugSliderProp;
    SerializedProperty EnableTouchSliderProp;

    SerializedProperty BasicSettingsTintColorProp;
    SerializedProperty BasicSettingsHueProp;
    SerializedProperty BasicSettingsSaturationProp;
    SerializedProperty BasicSettingsBrightnessProp;
    SerializedProperty BasicSettingsContrastProp;
    SerializedProperty BasicSettingsSharpnessProp;
    
    SerializedProperty GrainSettingsContributionProp;
    SerializedProperty GrainSettingsSizeProp;

    SerializedProperty VignetteSettingsContributionProp;
    SerializedProperty VignetteSettingsColorProp;

    SerializedProperty ScanlineSettingsContributionProp;
    SerializedProperty ScanlineSettingsColorProp;
    SerializedProperty ScanlineSettingsScanlineXProp;
    SerializedProperty ScanlineSettingsScanlineYProp;

    private void OnEnable()
    {
        LutModeProp = serializedObject.FindProperty("LutMode");
        LutContributionProp = serializedObject.FindProperty("LutContribution");
        LookupTextureProp = serializedObject.FindProperty("LookupTexture");

        PixelSizeProp = serializedObject.FindProperty("PixelSize");
        CelCutsProp = serializedObject.FindProperty("CelCuts");

        DebugSliderProp = serializedObject.FindProperty("DebugSlider");
        EnableTouchSliderProp = serializedObject.FindProperty("EnableTouchSlider");

        BasicSettingsTintColorProp = serializedObject.FindProperty("BasicSettings.TintColor");
        BasicSettingsHueProp = serializedObject.FindProperty("BasicSettings.Hue");
        BasicSettingsSaturationProp = serializedObject.FindProperty("BasicSettings.Saturation");
        BasicSettingsBrightnessProp = serializedObject.FindProperty("BasicSettings.Brightness");
        BasicSettingsContrastProp = serializedObject.FindProperty("BasicSettings.Contrast");
        BasicSettingsSharpnessProp = serializedObject.FindProperty("BasicSettings.Sharpness");

        GrainSettingsContributionProp = serializedObject.FindProperty("GrainSettings.Contribution");
        GrainSettingsSizeProp = serializedObject.FindProperty("GrainSettings.Size");

        VignetteSettingsContributionProp = serializedObject.FindProperty("VignetteSettings.Contribution");
        VignetteSettingsColorProp = serializedObject.FindProperty("VignetteSettings.Color");


        ScanlineSettingsContributionProp = serializedObject.FindProperty("ScanlineSettings.Contribution");
        ScanlineSettingsColorProp = serializedObject.FindProperty("ScanlineSettings.Color");
        ScanlineSettingsScanlineXProp = serializedObject.FindProperty("ScanlineSettings.ScanlineX");
        ScanlineSettingsScanlineYProp = serializedObject.FindProperty("ScanlineSettings.ScanlineY");
    }

    public override void OnInspectorGUI()
    {
        if (FC == null) FC = (FMColorFast)target;
        if (logo == null) logo = Resources.Load<Texture2D>("Logo/" + "Logo_FMColor");
        if (logo != null)
        {
            const float maxLogoWidth = 430.0f;
            EditorGUILayout.Separator();
            float w = EditorGUIUtility.currentViewWidth;
            Rect r = new Rect();
            r.width = Math.Min(w - 40.0f, maxLogoWidth);
            r.height = r.width / 4.886f;
            Rect r2 = GUILayoutUtility.GetRect(r.width, r.height);
            r.x = r2.x;
            r.y = r2.y;
            GUI.DrawTexture(r, logo, ScaleMode.ScaleToFit);
            if (GUI.Button(r, "", new GUIStyle())) Application.OpenURL("http://frozenmist.com");
            EditorGUILayout.Separator();
        }

        serializedObject.Update();

        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        //Templates
        GUILayout.BeginVertical("box");
        {
            if (!ShowPreset)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Templates")) ShowPreset = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("- Templates")) ShowPreset = false;
                GUILayout.EndHorizontal();

                GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(FC.LutMode.ToString());
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Minimum")) FC.Action_TemplateMinimum();
                if (GUILayout.Button("Mono")) FC.Action_TemplateMono();
                if (GUILayout.Button("Toon")) FC.Action_TemplateToon();
                if (GUILayout.Button("Contrast")) FC.Action_TemplateContrast();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Scanline")) FC.Action_TemplateScanline();
                if (GUILayout.Button("Grid")) FC.Action_TemplateGrid();
                if (GUILayout.Button("Pixelate")) FC.Action_TemplatePixelate();
                if (GUILayout.Button("Film Grain")) FC.Action_TemplateFilmGrain();
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();

        //Shortcuts
        GUILayout.BeginHorizontal("box");
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(" < ")) FC.Action_PreviousLUT();
            GUILayout.Label(" Change LUT ");
            if (GUILayout.Button(" > ")) FC.Action_NextLUT();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();

        //Lut
        GUILayout.BeginVertical("box");
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(LutContributionProp, new GUIContent("LUT Contribution"));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(LutModeProp, new GUIContent("LUT Mode"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(LookupTextureProp, new GUIContent("Look Up Texture"));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        //Cel
        GUILayout.BeginVertical("box");
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(PixelSizeProp, new GUIContent("Pixel Size"));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(CelCutsProp, new GUIContent("Cel Cuts"));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        //Basic Settings
        GUILayout.BeginVertical("box");
        {
            if (!ShowBasicSettings)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Basic Settings")) ShowBasicSettings = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("- Basic Settings")) ShowBasicSettings = false;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(BasicSettingsTintColorProp, new GUIContent("Tint Color"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(BasicSettingsHueProp, new GUIContent("Hue"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(BasicSettingsSaturationProp, new GUIContent("Saturation"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(BasicSettingsBrightnessProp, new GUIContent("Brightness"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(BasicSettingsContrastProp, new GUIContent("Contrast"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(BasicSettingsSharpnessProp, new GUIContent("Sharpness"));
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();

        //Grain Settings
        GUILayout.BeginVertical("box");
        {
            if (!ShowGrainSettings)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Grain Settings")) ShowGrainSettings = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("- Grain Settings")) ShowGrainSettings = false;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(GrainSettingsContributionProp, new GUIContent("Contribution"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(GrainSettingsSizeProp, new GUIContent("Size"));
                GUILayout.EndHorizontal();

            }
        }
        GUILayout.EndVertical();

        //Vignette Settings
        GUILayout.BeginVertical("box");
        {
            if (!ShowVignetteSettings)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Vignette Settings")) ShowVignetteSettings = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("- Vignette Settings")) ShowVignetteSettings = false;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(VignetteSettingsContributionProp, new GUIContent("Contribution"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(VignetteSettingsColorProp, new GUIContent("Color"));
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();

        //Scanline Settings
        GUILayout.BeginVertical("box");
        {
            if (!ShowScanlineSettings)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Scanline Settings")) ShowScanlineSettings = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("- Scanline Settings")) ShowScanlineSettings = false;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ScanlineSettingsContributionProp, new GUIContent("Contribution"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ScanlineSettingsColorProp, new GUIContent("Color"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ScanlineSettingsScanlineXProp, new GUIContent("Scanline X"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ScanlineSettingsScanlineYProp, new GUIContent("Scanline Y"));
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();

        //Debug
        GUILayout.BeginVertical("box");
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(DebugSliderProp, new GUIContent("Debug Slider"));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(EnableTouchSliderProp, new GUIContent("Touch Slide"));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
