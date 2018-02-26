실행방법
1. Coin.exe, Newtonsoft.Json.dll, log4net.dll 등을 같은 폴더에 복사한다.
2. Coin.exe 실행
3. 마우스 오른쪽 버튼 클릭시 종료됨

옵션 설정
- res 폴더에 Config.json, LogConfig.xml 파일을 복사하면 된다.
- Config.json
 > sourceUrl : 시세를 가져오는 url
 > updateInterval : 시세 업데이트 간격(초), 30초 이상만 가능
- LogConfig.xml
 > 위 파일이 있을 경우 log폴더에 로그가 생성되어 프로그램 오류 추적 가능
