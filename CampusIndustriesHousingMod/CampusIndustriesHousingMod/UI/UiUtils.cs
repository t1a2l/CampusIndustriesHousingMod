using ColossalFramework.UI;
using UnityEngine;

namespace CampusIndustriesHousingMod.UI 
{
	public static class UiUtils 
	{
		public static UIButton CreateButton(UIComponent parent, string name)
        {
            UIButton button = parent.AddUIComponent<UIButton>();
            button.name = name;
            button.autoSize = true;
            return button;
        }

        public static UICheckBox CreateCheckBox(UIComponent parent, string name, string text, bool state)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.name = name;

            checkBox.height = 16f;
            checkBox.width = parent.width - 10f;

            UISprite uncheckedSprite = checkBox.AddUIComponent<UISprite>();
            uncheckedSprite.spriteName = "check-unchecked";
            uncheckedSprite.size = new Vector2(16f, 16f);
            uncheckedSprite.relativePosition = Vector3.zero;

            UISprite checkedSprite = checkBox.AddUIComponent<UISprite>();
            checkedSprite.spriteName = "check-checked";
            checkedSprite.size = new Vector2(16f, 16f);
            checkedSprite.relativePosition = Vector3.zero;
            checkBox.checkedBoxObject = checkedSprite;

            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.text = text;
            checkBox.label.font = GetUIFont("OpenSans-Regular");
            checkBox.label.autoSize = false;
            checkBox.label.height = 20f;
            checkBox.label.verticalAlignment = UIVerticalAlignment.Middle;
            checkBox.label.relativePosition = new Vector3(20f, 0f);

            checkBox.isChecked = state;

            return checkBox;
        }

        public static UIFont GetUIFont(string name)
        {
            UIFont[] fonts = Resources.FindObjectsOfTypeAll<UIFont>();

            foreach (UIFont font in fonts)
            {
                if (font.name.CompareTo(name) == 0)
                {
                    return font;
                }
            }

            return null;
        }

        public static UILabel CreateLabel(UIComponent parent, string name, string text, string prefix)
        {
            UILabel label = parent.AddUIComponent<UILabel>();
            label.name = name;
            label.text = text;
            label.prefix = prefix;

            return label;
        }

        public static UIPanel CreatePanel(UIComponent parent, string name)
        {
            UIPanel panel = parent.AddUIComponent<UIPanel>();
            panel.name = name;

            return panel;
        }

        public static UITextField CreateTextField(UIComponent parent, string name, string tooltip)
		{
			UITextField textField = parent.AddUIComponent<UITextField>();
            textField.name = name;
            textField.size = new Vector2(90f, 17f);
            textField.padding = new RectOffset(0, 0, 9, 3);
            textField.builtinKeyNavigation = true;
            textField.isInteractive = true;
            textField.readOnly = false;
            textField.horizontalAlignment = UIHorizontalAlignment.Center;
            textField.verticalAlignment = UIVerticalAlignment.Middle;
            textField.selectionSprite = "EmptySprite";
            textField.selectionBackgroundColor = new Color32(233, 201, 148, 255);
            textField.normalBgSprite = "TextFieldPanelHovered";
            textField.disabledBgSprite = "TextFieldPanel";
            textField.textColor = new Color32(0, 0, 0, 255);
            textField.disabledTextColor = new Color32(0, 0, 0, 128);
            textField.color = new Color32(185, 221, 254, 255);
            textField.tooltip = tooltip;
            textField.size = new Vector2(150f, 27f);
            textField.padding.top = 2;
            textField.numericalOnly = true;
            textField.allowNegative = false;
            textField.allowFloats = false;
            textField.multiline = false;

            return textField;
		}

        public static Vector3 PositionUnder(UIComponent uIComponent, float margin = 8f, float horizontalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + horizontalOffset, uIComponent.relativePosition.y + uIComponent.height + margin);
        }

        public static Vector3 PositionRightOf(UIComponent uIComponent, float margin = 8f, float verticalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }

        public static UIPanel UIServiceBar(UIComponent parent, string name, string text, string prefix, string tooltip)
        {
            float DEFAULT_SCALE = 0.8f;
            // panel
            UIPanel m_uiPanel = parent.AddUIComponent<UIPanel>();
            m_uiPanel.name = name;
            m_uiPanel.height = 20f;

            // text
            var label_name = name + "Label";
            UILabel m_uiTextLabel = CreateLabel(m_uiPanel, label_name, text, prefix);
            m_uiTextLabel.textAlignment = UIHorizontalAlignment.Left;
            m_uiTextLabel.relativePosition = new Vector3(0, 0);
            m_uiTextLabel.textScale = DEFAULT_SCALE;

            // value
            var text_name = name + "Textfield";
            UITextField m_uiValueLabel = CreateTextField(m_uiPanel, text_name, tooltip);
            m_uiValueLabel.name = name + "Value";
            m_uiValueLabel.textScale = DEFAULT_SCALE;
            m_uiValueLabel.relativePosition = new Vector3(135f, 0f);

            return m_uiPanel;
        }
	}
}
