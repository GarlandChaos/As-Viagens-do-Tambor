public interface IScreenController
{
    string screenID { get; set; }
    bool isVisible { get; set; }
    void Show();
    void Hide();
}

public interface IPanelController : IScreenController
{

}

public interface IDialogController : IScreenController
{

}