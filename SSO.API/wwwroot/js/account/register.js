var RegisterAccountView = /** @class */ (function () {
    function RegisterAccountView() {
    }
    return RegisterAccountView;
}());
var RegisterComponent = /** @class */ (function () {
    function RegisterComponent() {
    }
    RegisterComponent.prototype.register = function () {
        debugger;
        var model = new RegisterAccountView();
        model.email = document.getElementById("email").value;
        model.password = document.getElementById("password").value;
        model.confirmPassword = document.getElementById("confirmPassword").value;
        model.returnUrl = document.getElementById("returnUrl").value;
        var xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/api/account/register", true);
        xhttp.onreadystatechange = function () {
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
    };
    return RegisterComponent;
}());
var registerVM = new RegisterComponent();
//# sourceMappingURL=register.js.map