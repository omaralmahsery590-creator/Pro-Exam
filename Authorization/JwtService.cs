using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace pro_exam.Authorization;
public class JwtService
{

    public static string DecodeJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();// يستخدم لتحليل وفك تشفير JWT token
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;// قراءة ال token وتحليله إلى كائن JwtSecurityToken

        if (jsonToken == null)// التحقق مما إذا كان التحليل ناجحًا، وإذا لم يكن كذلك، يتم رمي استثناء
            throw new ArgumentException("Invalid JWT token");// رمي استثناء إذا كان ال token غير صالح

        var payload = jsonToken.Claims// استخراج الادعاءات (claims) من ال token وتحويلها إلى قاموس (dictionary) حيث يكون نوع الادعاء هو المفتاح وقيمة الادعاء هي القيمة
            .Select(c => new { c.Type, c.Value })// اختيار نوع الادعاء وقيمته
            .ToDictionary(c => c.Type, c => c.Value);// تحويل الادعاءات إلى قاموس (dictionary) حيث يكون نوع الادعاء هو المفتاح وقيمة الادعاء هي القيمة

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });// تحويل القاموس إلى سلسلة JSON منسقة بشكل جميل (indented) باستخدام JsonSerializer
    }



}
