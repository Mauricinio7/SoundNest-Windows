using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Utilities
{
    internal static class Utilities
    {
        public const string LABEL_STYLE = "LabelStyle";
        public const string TEXT = "Text";
        public const string PLACEHOLDER_TEXT = "PlaceholderText";
        public const string FONT_SIZE_VALUE = "FontSizeValue";
        public const string BACKGROUND_COLOR_VALUE = "BackgroundColorValue";
        public const string PLACEHOLDER_FOREGROUND_COLOR_VALUE = "PlaceholderForegroundColorValue";
        public const string WRITING_FOREGROUND_COLOR_VALUE = "WritingForegroundColorValue";
        public const string PADDING_VALUE = "PaddingValue";
        public const string BORDER_THICKNESS_VALUE = "BorderThicknessValue";
        public const string WIDTH = "Width";
        public const string HEIGHT = "Height";
        public const string PASSWORD = "Password";
        public const string TEXT_BOX_WIDTH = "TextBoxWidth";
        public const string TEXT_BOX_HEIGHT = "TextBoxHeight";
        public const string CHECK_BOX_MARGIN = "CheckBoxMargin";
        public const string PASSWORD_REGEX = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,25}$";
        public const string USERNAME_REGEX = @"^[a-zA-Z0-9_]{3,25}$";
        public const string EMAIL_REGEX = @"^(?=.{1,100}$)[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";


    }
}
