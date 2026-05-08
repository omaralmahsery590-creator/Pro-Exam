using System.IdentityModel.Tokens.Jwt;

namespace pro_exam.Authorization
{
    public class InformationToken
    {

        public static JwtAuthResponse GetInfoUsers(string strToken)// استقبال ال token من ال header وتحليلها لاستخراج المعلومات منها
        {
            var handler = new JwtSecurityTokenHandler();//يستخدم لتحليل وفك تشفير JWT token
            var JwtToken = handler.ReadJwtToken(strToken.Replace("Bearer ", ""));// إزالة "Bearer " من بداية ال token قبل تحليله
            var InfoDecrypt = SecuredHelper.GetInfoFromToken(JwtToken.ToString());// استخراج المعلومات من ال token باستخدام دالة GetInfoFromToken في SecuredHelper
            return InfoDecrypt;
        }
    }
}
