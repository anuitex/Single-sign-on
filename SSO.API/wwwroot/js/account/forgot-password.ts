class ForgotPasswordViewModel {
    public email: string;
}
class ForgoPasswordComponent {
    public forgotPassword() {
        let model = new ForgotPasswordViewModel();
        model.email = (<any>document.getElementById("email")).value;
        let xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/api/account/forgotPassword", true);
        xhttp.onreadystatechange = () => {
            if (xhttp.readyState == 4 && xhttp.status == 200) {
                window.location.href = '/Account/ForgotPasswordConfirmation';
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
var forgotPassword = new ForgoPasswordComponent();