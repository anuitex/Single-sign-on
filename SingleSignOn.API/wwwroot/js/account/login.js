var LoginAccountView = /** @class */ (function () {
    function LoginAccountView() {
    }
    return LoginAccountView;
}());
var LoginComponent = /** @class */ (function () {
    function LoginComponent() {
    }
    LoginComponent.prototype.login = function () {
        var model = new LoginAccountView();
        model.Username = document.getElementById("email").value;
        model.Password = document.getElementById("password").value;
        var xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/api/account/login", true);
        xhttp.onreadystatechange = function () {
            if (xhttp.readyState == 4 && xhttp.status == 200) {
                var data = JSON.parse(xhttp.response);
                if (data.userInfo.token && data.userInfo.token != '') {
                    localStorage.setItem('user-info', JSON.stringify(data.userInfo));
                }
                window.location.href = data.returnUrl;
                return;
            }
            if (xhttp.readyState == 4 && xhttp.status == 400) {
                document.getElementById("validation-errors").innerHTML = '<div class="text-danger">' + xhttp.response + '</div>';
            }
        };
        xhttp.setRequestHeader("Content-type", "application/json");
        xhttp.send(JSON.stringify(model));
    };
    return LoginComponent;
}());
var login = new LoginComponent();
//# sourceMappingURL=login.js.map