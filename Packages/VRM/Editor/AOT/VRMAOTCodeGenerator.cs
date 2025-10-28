namespace VRM
{
    public class VRMAOTCodeGenerator
    {
        public static void GenerateCode()
        {
            VRMSerializerGenerator.GenerateCode();
            VRMDeserializerGenerator.GenerateCode();
        }
    }
}