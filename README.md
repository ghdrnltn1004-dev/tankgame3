# TankGame3 인수인계

Unity 프로젝트: TankGame3
Unity 버전: 6000.3.10f1에서 작업/검증함

## 집 컴퓨터에서 이어서 작업하는 법
1. 이 저장소를 집 컴퓨터에 clone 또는 Download ZIP으로 받기
2. Unity Hub에서 폴더 열기
3. Unity가 외부 수정/패키지 임포트/에셋 재임포트를 물으면 진행
4. `Assets/Scenes/SampleScene.unity` 열기
5. Play 실행

## 이번 학교 PC에서 적용한 주요 내용
- TankGame5 기능 이식: 총알 발사, 연사, 피격 효과, 사운드, 장애물 파괴/리스폰, 적 포탑
- 적 포탑 수 2개로 제한
- 플레이어 탱크 피격/체력/파괴 후 씬 리로드
- Archery Target / Dummy Target FREE 적용
- Archery Target: 위아래 이동, 파괴 시 10점
- Dummy Target FREE: 자유 이동, 파괴 시 20점
- 점수 UI 추가
- EnvLevel 환경맵 임포트: Desert/Jungle/Moon
- 기본 씬에는 Desert 맵 자동 생성
- 사막 바닥 깨짐 방지용 Stable Desert Floor 추가

## 주요 스크립트 위치
- `Assets/scripts/SampleTankWeapon.cs`: 탱크 발사/피격/파괴
- `Assets/scripts/Bullet.cs`: 총알 충돌 처리
- `Assets/scripts/TargetDestroy.cs`: 타겟 파괴/리스폰/점수
- `Assets/scripts/SampleTargetSetBuilder.cs`: Archery/Dummy 타겟 자동 배치
- `Assets/scripts/SampleEnvironmentLevelBuilder.cs`: EnvLevel 맵 자동 배치 및 바닥 보정
- `Assets/scripts/SampleEnemySpawner.cs`: 적 포탑 자동 보충
- `Assets/scripts/ScoreManager.cs`: 점수 표시

## 참고
- `Library`, `Temp`, `Logs`, `.vs` 폴더는 Unity가 다시 생성하므로 GitHub에 올리지 않음
- 기존 `TankGame.zip` 같은 백업 압축파일도 저장소에서는 제외함
