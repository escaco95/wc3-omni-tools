using System.Diagnostics;
using System.Threading;

namespace NonWPF.Forms
{
    public static class SingleInstanceUtils
    {
        private static Mutex? _mutex;

        /// <summary>
        /// 애플리케이션의 싱글 인스턴스 실행을 보장합니다.
        /// </summary>
        /// <param name="appName">고유한 애플리케이션 이름</param>
        /// <returns>애플리케이션이 첫 번째 인스턴스일 경우 true, 이미 실행 중이면 false</returns>
        public static bool EnsureSingleInstance(string appName)
        {
            // Mutex 이름을 제공된 appName으로 설정하여 고유한 인스턴스를 식별합니다.
            string mutexName = $"{appName}_Mutex";

            // Mutex 객체를 생성합니다.
            _mutex = new Mutex(true, mutexName, out bool isNewInstance);

            // 이미 실행 중인 인스턴스가 있다면 false를 반환합니다.
            if (!isNewInstance)
            {
                _mutex.Dispose(); // 생성된 두 번째 Mutex 해제
                _mutex = null; // 두 번째 인스턴스에서는 Mutex를 유지하지 않음

                Debug.WriteLine("이미 실행 중인 인스턴스가 있습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 애플리케이션 종료 시 Mutex 해제.
        /// </summary>
        public static void ReleaseMutex()
        {
            _mutex?.ReleaseMutex();
        }
    }
}
