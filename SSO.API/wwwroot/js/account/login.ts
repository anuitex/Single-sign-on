class LoginAccountView {
    public email: string;
    public password: string;
    public returnUrl: string;
}
class LoginComponent {
    public login() {
        let model = new LoginAccountView();
        model.email = (<any>document.getElementById("email")).value;
        model.password = (<any>document.getElementById("password")).value;
        model.returnUrl = (<any>document.getElementById("returnUrl")).value;
        let xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/api/account/login", true);
        xhttp.onreadystatechange = () => {
            if (xhttp.readyState == 4 && xhttp.status == 200) {
                let data: any = JSON.parse(xhttp.response);
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
    }
}
var login = new LoginComponent();