var LoginPartialComponent = /** @class */ (function () {
    function LoginPartialComponent() {
    }
    LoginPartialComponent.prototype.login = function () {
        var login = document.getElementById("email").value;
        var password = document.getElementById("password").value;
        var returnUrl = document.getElementById("returnUrl").value;
        var xhttp = new XMLHttpRequest();
        xhttp.open("POST", "api/account/login", true);
        xhttp.onreadystatechange = function () {
            if (xhttp.readyState == 4 && xhttp.status == 200) {
                var returnUrl_1 = xhttp.response;
                window.location.href = returnUrl_1;
                return;
            }
            if (xhttp.readyState == 4 && xhttp.status == 400) {
                document.getElementById("validation-errors").innerHTML = '<div class="text-danger">' + xhttp.response + '</div>';
            }
        };
        xhttp.setRequestHeader("Content-type", "application/json");
        var model = new LoginAccountView();
        model.Email = login;
        model.Password = password;
        model.ReturnUrl = returnUrl;
        xhttp.send(JSON.stringify(model));
    };
    return LoginPartialComponent;
}());
var login = new LoginComponent();
//# sourceMappingURL=Login.js.map