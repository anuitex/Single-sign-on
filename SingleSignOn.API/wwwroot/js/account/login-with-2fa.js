var LoginWith2faViewModel = /** @class */ (function () {
    function LoginWith2faViewModel() {
    }
    return LoginWith2faViewModel;
}());
var LoginWith2faComponent = /** @class */ (function () {
    function LoginWith2faComponent() {
    }
    LoginWith2faComponent.prototype.login = function () {
        debugger;
        var model = new LoginWith2faViewModel();
        model.twoFactorCode = document.getElementById("twoFactorCode").value;
        model.rememberMachine = document.getElementById("RememberMachine").value;
        model.userId = document.getElementById("userId").value;
        model.returnUrl = document.getElementById("returnUrl").value;
        var xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/api/account/loginWith2fa", true);
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
    return LoginWith2faComponent;
}());
var loginWith2faComponent = new LoginWith2faComponent();
//# sourceMappingURL=login-with-2fa.js.map