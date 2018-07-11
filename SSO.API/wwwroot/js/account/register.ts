class RegisterAccountView {
    public email: string;
    public password: string;
    public confirmPassword: string;
    public returnUrl: string;
}
class RegisterComponent {
    public register() {
        debugger;
        let model = new RegisterAccountView();
        model.email = (<any>document.getElementById("email")).value;
        model.password = (<any>document.getElementById("password")).value;
        model.confirmPassword = (<any>document.getElementById("confirmPassword")).value;
        model.returnUrl = (<any>document.getElementById("returnUrl")).value;
        let xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/api/account/register", true);
        xhttp.onreadystatechange = () => {
            if (xhttp.readyState == 4 && xhttp.status == 200) {
                window.location.href = xhttp.response;
                return;
            }
            if (xhttp.readyState == 4 && xhttp.status == 400) {
                document.getElementById("validation-errors").innerHTML = '<div class="text-danger">' + xhttp.response + '</div>';
            }
        };
        xhttp.setRequestHeader("Content-type", "application/json");
        xhttp.send(JSON.stringify(model));
    }
}
var registerVM = new RegisterComponent();