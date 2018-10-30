var ForgotPasswordViewModel = /** @class */ (function () {
    function ForgotPasswordViewModel() {
    }
    return ForgotPasswordViewModel;
}());
var ForgoPasswordComponent = /** @class */ (function () {
    function ForgoPasswordComponent() {
    }
    ForgoPasswordComponent.prototype.forgotPassword = function () {
        var model = new ForgotPasswordViewModel();
        model.email = document.getElementById("email").value;
        var xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/api/account/forgotPassword", true);
        xhttp.onreadystatechange = function () {
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
    };
    return ForgoPasswordComponent;
}());
var forgotPassword = new ForgoPasswordComponent();
//# sourceMappingURL=forgot-password.js.map