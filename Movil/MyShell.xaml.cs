using Movil.Pages;

namespace Movil;

public partial class MyShell : Shell
{
	public MyShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("Password", typeof(Password));


    }
}