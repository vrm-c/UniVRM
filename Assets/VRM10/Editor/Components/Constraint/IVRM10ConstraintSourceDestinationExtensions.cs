using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public static class IVRM10ConstraintSourceDestinationExtensions
    {
        public static void DrawSourceCoords(this IVRM10ConstraintSourceDestination self)
        {
            try
            {
                self.GetSourceCoords().Draw(0.2f);
            }
            catch (ConstraintException)
            {

            }
        }
        public static void DrawSourceCurrent(this IVRM10ConstraintSourceDestination self)
        {
            try
            {
                Handles.matrix = self.GetSourceCurrent().TRS(0.05f);
                Handles.color = Color.yellow;
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
            catch (ConstraintException)
            {

            }
        }

        public static void DrawDstCoords(this IVRM10ConstraintSourceDestination self)
        {
            try
            {
                self.GetDstCoords().Draw(0.2f);
            }
            catch (ConstraintException)
            {

            }
        }

        public static void DrawDstCurrent(this IVRM10ConstraintSourceDestination self)
        {
            try
            {
                Handles.matrix = self.GetDstCurrent().TRS(0.05f);
                Handles.color = Color.yellow;
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
            catch (ConstraintException)
            {

            }
        }
    }
}
