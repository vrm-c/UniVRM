namespace UniGLTF
{
    class AnimationKeyframeData
    {
#if UNITY_EDITOR
        public float Time { get; set; }
        public delegate float[] ConverterFunc(float[] values);
        private ConverterFunc _converter;
        private float[] _values;
        public float[] Values
        {
            get { return _values; }
        }

        private bool[] _enterValues;
        public bool[] EnterValues
        {
            get { return _enterValues; }
        }

        public AnimationKeyframeData(int elementCount, ConverterFunc converter)
        {
            _values = new float[elementCount];
            _enterValues = new bool[elementCount];
            for (int i = 0; i < _enterValues.Length; i++)
            {
                _enterValues[i] = false;
            }
            _converter = converter;
        }

        public void SetValue(float src, int offset)
        {
            if (_values.Length > offset)
            {
                _values[offset] = src;
                _enterValues[offset] = true;
            }
        }

        public virtual float[] GetRightHandCoordinate()
        {
            if (_converter != null)
            {
                return _converter(_values);
            }
            else
            {
                return _values;
            }
        }
#endif
    }
}