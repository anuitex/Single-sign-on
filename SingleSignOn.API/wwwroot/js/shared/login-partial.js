var LoginPartialComponent = /** @class */ (function () {
    function LoginPartialComponent() {
        var userJson = localStorage.getItem('user-info');
        if (!userJson) {
            document.getElementById('loginForm').classList.remove("hidden");
            return;
        }
        document.getElementById('logoutForm').classList.remove("hidden");
        var user = JSON.parse(userJson);
        document.getElementById('username').innerHTML = 'Hello ' + user.userName + '!';
    }
    LoginPartialComponent.prototype.logout = function () {
        localStorage.clear();
        window.location.reload();
    };
    return LoginPartialComponent;
}());
var loginPartial = new LoginPartialComponent();
//# sourceMappingURL=login-partial.js.map