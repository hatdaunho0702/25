# BÁO CÁO S?A L?I CSRF (ANTI-FORGERY TOKEN)

## Ngày th?c hi?n: ${new Date().toLocaleDateString('vi-VN')}

---

## ?? V?N Ð?

L?i: **"The required anti-forgery cookie '__RequestVerificationToken' is not present"**

Ðây là l?i b?o m?t CSRF (Cross-Site Request Forgery) x?y ra khi:
- Controller có attribute `[ValidateAntiForgeryToken]`
- Nhýng View không có `@Html.AntiForgeryToken()` trong form

---

## ? CÁC FILE Ð? S?A

### 1. Views ð? thêm AntiForgeryToken:

#### User Views
- ? `WebApplication15\Views\User\Register.cshtml`
- ? `WebApplication15\Views\User\Login.cshtml`
- ? `WebApplication15\Views\LienHe\Index.cshtml`
- ? `WebApplication15\Views\Home\ChiTietSP.cshtml` (form ðánh giá)

#### Admin Views (ð? có s?n, ki?m tra l?i)
- ? `WebApplication15\Areas\Admin\Views\SanPham\Create.cshtml`
- ? `WebApplication15\Areas\Admin\Views\ThuongHieu\Create.cshtml`
- ? `WebApplication15\Areas\Admin\Views\LienHes\Create.cshtml`
- ? `WebApplication15\Areas\Admin\Views\LienHes\Edit.cshtml`
- ? `WebApplication15\Areas\Admin\Views\LienHes\Index.cshtml` (bulk delete form)
- ? `WebApplication15\Areas\Admin\Views\NguoiDungs\Create.cshtml`

### 2. Controllers ð? thêm ValidateAntiForgeryToken:

- ? `WebApplication15\Controllers\HomeController.cs` - ThemDanhGia action

---

## ?? CODE THAY Ð?I

### Ví d? thay ð?i trong View:

**Trý?c:**
```razor
@using (Html.BeginForm("RegisterSubmit", "User", FormMethod.Post))
{
    <!-- form fields -->
    <button type="submit">ÐÃNG K?</button>
}
```

**Sau:**
```razor
@using (Html.BeginForm("RegisterSubmit", "User", FormMethod.Post))
{
    @Html.AntiForgeryToken()
    
    <!-- form fields -->
    <button type="submit">ÐÃNG K?</button>
}
```

### Ví d? thay ð?i trong Controller:

**Ð? có s?n:**
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult RegisterSubmit(FormCollection form)
{
    // code
}
```

---

## ??? B?O M?T

### Anti-Forgery Token ho?t ð?ng nhý th? nào?

1. **Server t?o token** khi render form v?i `@Html.AntiForgeryToken()`
2. **Token ðý?c g?i 2 nõi**:
   - Cookie: `__RequestVerificationToken`
   - Hidden field trong form
3. **Khi submit**, server ki?m tra:
   - Token trong cookie
   - Token trong form data
   - Hai token ph?i kh?p nhau

4. **Ngãn ch?n CSRF**: Attacker không th? gi? m?o request t? site khác v? không có token h?p l?

---

## ?? KI?M TRA

Ð? test các form sau:
- ? Ðãng k? (Register)
- ? Ðãng nh?p (Login)
- ? G?i liên h?
- ? Ðánh giá s?n ph?m
- ? Thêm/S?a/Xóa trong Admin

---

## ?? LÝU ? QUAN TR?NG

### 1. T?t c? POST forms ph?i có AntiForgeryToken

N?u b?n t?o form m?i sau này, nh?:

```razor
@using (Html.BeginForm(...))
{
    @Html.AntiForgeryToken()  // ? B?T BU?C
    
    <!-- form fields -->
}
```

### 2. AJAX Requests

N?u dùng AJAX POST, c?n thêm token vào header:

```javascript
$.ajax({
    url: '/Controller/Action',
    type: 'POST',
    data: formData,
    headers: {
        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
    }
});
```

### 3. Global Configuration

Có th? b?t ValidateAntiForgeryToken globally trong `FilterConfig.cs`:

```csharp
public static void RegisterGlobalFilters(GlobalFilterCollection filters)
{
    filters.Add(new HandleErrorAttribute());
    filters.Add(new ValidateAntiForgeryTokenAttribute());  // ? Global CSRF protection
}
```

**?? CHÚ ?**: N?u b?t global, t?t c? POST actions ð?u ph?i có token!

---

## ? K?T QU?

- ? **Build Status**: SUCCESSFUL
- ? **CSRF Protection**: ENABLED cho t?t c? forms
- ? **L?i Anti-Forgery**: Ð? S?A
- ? **Security**: IMPROVED

---

## ?? TÀI LI?U THAM KH?O

- [ASP.NET MVC Anti-CSRF Protection](https://docs.microsoft.com/en-us/aspnet/mvc/overview/security/xsrfcsrf-prevention-in-aspnet-mvc-and-web-pages)
- [OWASP CSRF Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html)

---

*Báo cáo ðý?c t?o b?i GitHub Copilot*
