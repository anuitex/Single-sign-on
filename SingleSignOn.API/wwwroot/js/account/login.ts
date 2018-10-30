class LoginAccountView {
    public Username: string;
    public Password: string;
}
class LoginComponent {
    public login() {
        let model = new LoginAccountView();
        model.Username = (<any>document.getElementById("email")).value;
        model.Password = (<any>document.getElementById("password")).value;
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