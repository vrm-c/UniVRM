namespace UniVRM10
{
    public interface IVRM10ConstraintSourceDestination
    {
        TR GetSourceCoords();
        TR GetSourceCurrent();

        TR GetDstCoords();
        TR GetDstCurrent();
    }
}
