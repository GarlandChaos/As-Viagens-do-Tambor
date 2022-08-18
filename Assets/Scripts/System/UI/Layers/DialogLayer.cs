using System.Collections.Generic;
using UnityEngine;

namespace TamborGame.UI
{
    public class DialogLayer : ALayer<IDialogController>
    {
        Stack<IDialogController> screenStack = new Stack<IDialogController>();

        public override void ShowScreen(string screenID)
        {
            if (!screenStack.Contains(screens[screenID]))
            {
                if (screenStack.Count > 0)
                {
                    IDialogController dialogControllerPeek = screenStack.Peek();
                    dialogControllerPeek.Hide();
                }
                screenStack.Push(screens[screenID]);
                screenStack.Peek().Show();
            }
#if UNITY_EDITOR
            else
                Debug.Log("SCREEN STACK ALREADY HAS " + screenID);
#endif
        }

        public override void HideScreen(string screenID)
        {
            if (screenStack.Contains(screens[screenID]))
            {
                screenStack.Peek().Hide();
                screenStack.Pop();

                if (screenStack.Count > 0)
                    screenStack.Peek().Show();
            }
#if UNITY_EDITOR
            else
                Debug.Log("SCREEN STACK DOESN'T HAVE " + screenID);
#endif
        }

        public override void SaySize()
        {
            Debug.Log("Dialog layer size is: " + screens.Count);
            base.SaySize();
        }

        public bool IsScreenOnStack(string screenID)
        {
            if (screenStack.Contains(screens[screenID]))
                return true;

            return false;
        }

        public override void ClearScreens()
        {
            if (screenStack.Count > 0)
            {
                foreach (IDialogController screen in screenStack)
                    screen.Hide();

                screenStack.Clear();
            }
        }
    }
}