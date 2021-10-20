using System;
using System.Text;

namespace DatabaseBuddy.CLI.DynamicCLIMenu
{
    public class DynamicCLIMenu
    {
        #region - needs -
        private const string SELECTED_ITEM = "[X]";
        private const string NOT_SELECTED_ITEM = "[ ]";
        private const string CONTINUE = "Continue";
        private const string CURSOR = ">";
        #endregion

        #region - ctor -
        public DynamicCLIMenu(string Title, List<string> Options)
        {
            m_Options = new Dictionary<string, bool>();
            foreach (var Item in Options.Distinct())
                m_Options.Add(Item, false);
            m_CursorPosition = 0;
        }
        #endregion

        #region - properties -

        #region - private properties -
        private string m_Title { get; set; }
        private Dictionary<string, bool> m_Options { get; set; }
        private int m_CursorPosition { get; set; }
        #endregion

        #region - public properties -

        #endregion

        #endregion

        #region - methods -

        #region - public properties -

        #region [Run]
        public Dictionary<string, bool> Run()
        {
            __PrintOptions();
            return m_Options;
        }
        #endregion

        #endregion

        #region - private methods -

        #region [__PrintOptions]
        private void __PrintOptions()
        {
            StringBuilder sb = new StringBuilder();
            Console.Clear();
            int Counter = 0;
            foreach (var Item in m_Options)
            {
                sb.Append(Counter == m_CursorPosition ? "> " : "  ");
                sb.Append(Item.Value ? SELECTED_ITEM : NOT_SELECTED_ITEM);
                sb.AppendLine($" {Item.Key}");
                Counter++;
            }
            Counter++;
            sb.Append(Counter == m_CursorPosition ? "> " : "  ");
            sb.AppendLine($"Continue");
            Console.Write(sb.ToString());
            __ExecuteInput();
        }
        #endregion

        #region [__ExecuteInput]
        private void __ExecuteInput()
        {
            var PressedKey = Console.ReadKey().Key;
            switch (PressedKey)
            {
                case ConsoleKey.UpArrow:
                    m_CursorPosition--;
                    break;
                case ConsoleKey.DownArrow:
                    m_CursorPosition++;
                    break;
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    if (m_CursorPosition != m_Options.Count)
                    {
                        m_Options[m_Options.ElementAt(m_CursorPosition).Key] = !m_Options.ElementAt(m_CursorPosition).Value;
                        break;
                    }
                    else
                        return;
            }
            if (m_CursorPosition < 0)
                m_CursorPosition = 0;
            else if (m_CursorPosition > m_Options.Count + 1)
                m_CursorPosition = m_CursorPosition = m_Options.Count + 1;
            __PrintOptions();
        }
        #endregion

        #endregion

        #endregion
    }
}

