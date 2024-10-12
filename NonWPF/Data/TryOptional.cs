using System;

namespace NonWPF.Data
{
    public class TryOptional<T>
    {
        private readonly bool _isPresent;
        private readonly T? _value;

        private TryOptional(bool isPresent, T? value)
        {
            _isPresent = isPresent;
            _value = value;
        }

        public static TryOptional<T> Of(Func<T> supplier)
        {
            try
            {
                var value = supplier.Invoke();
                return new TryOptional<T>(true, value);
            }
            catch
            {
                return new TryOptional<T>(false, default);
            }
        }

        public T OrElse(T other)
        {
            return (_isPresent && _value is T value) ? value : other;
        }
    }
}
