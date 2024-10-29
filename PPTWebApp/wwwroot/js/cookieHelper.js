window.cookieHelper = {
    setCookie: function (name, value, days) {
        const expires = new Date(Date.now() + days * 864e5).toUTCString();
        document.cookie = `${name}=${value}; expires=${expires}; path=/`;
    },
    getCookie: function (name) {
        return document.cookie.split('; ').find(row => row.startsWith(name + '='))?.split('=')[1] || "";
    },
    deleteCookie: function (name) {
        document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
    },
    clearAllCookies: function () {
        document.cookie.split(";").forEach(function (cookie) {
            document.cookie = cookie.replace(/^ +/, "").replace(/=.*/, "=;expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/");
        });
    }
};
