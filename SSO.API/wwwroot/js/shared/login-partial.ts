class LoginPartialComponent {
    constructor() {
        var userJson = localStorage.getItem('user-info');
        if (!userJson) {
            document.getElementById('loginForm').classList.remove("hidden");
            return;
        }
        document.getElementById('logoutForm').classList.remove("hidden");
        let user: UserInfo = JSON.parse(userJson);
        document.getElementById('username').innerHTML = 'Hello ' + user.userName + '!';
    }

    logout() {
        localStorage.clear();
        window.location.reload();
    }
}
var loginPartial = new LoginPartialComponent();