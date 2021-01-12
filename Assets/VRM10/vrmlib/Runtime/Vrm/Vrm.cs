namespace VrmLib
{
    public class Vrm
    {
        public Meta Meta = new Meta();

        public string ExporterVersion;
        public string SpecVersion;

        public Vrm(Meta meta, string exporterVersion, string specVersion)
        {
            Meta = meta;
            ExporterVersion = exporterVersion;
            SpecVersion = specVersion;
        }

        public override string ToString()
        {
            return $"{Meta}";
        }

        public ExpressionManager ExpressionManager = new ExpressionManager();

        public SpringBoneManager SpringBone = new SpringBoneManager();

        public FirstPerson FirstPerson = new FirstPerson();

        public LookAt LookAt = new LookAt();
    }
}
