using System;

namespace Svelto.ECS.GUI
{
    public sealed class GUIParseVariableService
    {
        internal GUIParseVariableService(GUIBlackboard guiBlackboard, IGUILocalizationService localizationService)
        {
            _guiBlackboard       = guiBlackboard;
            _localizationService = localizationService;
        }

        public void ParseVariables(ref string text, GUIComponent gui)
        {
            text = _localizationService.Localize(text);

            var data = _guiBlackboard.GetData(gui);
            var searchIndex = 0;
            var parsed      = false;
            while (!parsed)
            {
                // NOTE: The format for interpolated variables in strings is "{{variableName}}" this code will look for
                // this pattern and replace that string by the string value found in the WidgetInstanceData by the key
                // 'variableName'.
                var openVariableIndex = text.IndexOf("{{", searchIndex, StringComparison.Ordinal);
                if (openVariableIndex >= 0)
                {
                    searchIndex = openVariableIndex + 2;
                    var closeVariableIndex = text.IndexOf("}}", searchIndex, StringComparison.Ordinal);
                    if (closeVariableIndex - searchIndex > 0)
                    {
                        var variableName = text.Substring(searchIndex, closeVariableIndex - searchIndex);
                        if (data.TryGetValue(variableName, out string variableValue))
                        {
                            variableValue = _localizationService.Localize(variableValue);
                            // NOTE: The following line replaces {{variableName}} by variableValue.
                            // The reason for so many { } is because we are escaping double brackets {{ {{,
                            // plus one extra for the variableName interpolation {variableName}
                            text = text.Replace($"{{{{{variableName}}}}}", variableValue);
                        }
                        searchIndex = closeVariableIndex + 2;
                    }
                    else
                    {
                        parsed = true;
                    }
                }
                else
                {
                    parsed = true;
                }

                parsed |= searchIndex >= text.Length;
            }
        }

        readonly GUIBlackboard          _guiBlackboard;
        readonly IGUILocalizationService _localizationService;
    }
}