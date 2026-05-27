using Movil.Pages;

namespace Movil;

public partial class MyShell : Shell
{
	public MyShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("Registro", typeof(Registro));
        Routing.RegisterRoute("Password", typeof(Password));


    }
}