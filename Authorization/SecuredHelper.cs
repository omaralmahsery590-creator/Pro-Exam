using Nancy.Json;
namespace pro_exam.Authorization
{
    public class SecuredHelper//هنا تعريف الكلاس اللي راح يحتوي على الدوال اللي راح تستخدم لاستخراج البيانات من التوكن
    {
        public static JwtAuthResponse GetInfoFromToken(string Token)//هنا تعريف الدالة اللي راح تستخدم لاستخراج البيانات من التوكن
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();//هنا تعريف المتغير اللي راح يستخدم لتحويل البيانات من صيغة جيسون الى كلاس
            string[] source = Token.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);//هنا تعريف المتغير اللي راح يستخدم لتقسيم التوكن الى ثلاثة اجزاء
            var data = source[1] + source[2];//هنا تعريف المتغير اللي راح يستخدم لتجميع الجزئين الثاني والثالث من التوكن
            JwtAuthResponse authResponse = serializer.Deserialize<JwtAuthResponse>(data);//هنا تعريف المتغير اللي راح يستخدم لتخزين البيانات المستخرجة من التوكن بعد تحويلها من جيسون الى كلاس
            return authResponse;//هنا ارجاع البيانات المستخرجة من التوكن
        }
    }
}
