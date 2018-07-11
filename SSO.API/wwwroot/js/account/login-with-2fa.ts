class LoginWith2faViewModel {
    public twoFactorCode: string;
    public rememberMachine: boolean;
    public userId: number;
    public returnUrl: string;
}
class LoginWith2faComponent {
    public login() {
        debugger;
        let model = new LoginWith2faViewModel();
        model.twoFactorCode = (<any>document.getElementById("twoFactorCode")).value;
        model.rememberMachine = (<any>document.getElementById("RememberMachine")).value;
        model.userId = (<any>document.getElementById("userId")).value;
        model.returnUrl = (<any>document.getElementById("returnUrl")).value;
        let xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/api/account/loginWith2fa", true);
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
var loginWith2faComponent = new LoginWith2faComponent();