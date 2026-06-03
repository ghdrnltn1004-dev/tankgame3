# TankGame3 프로젝트 인수인계

Unity 프로젝트: TankGame3  
Unity 버전: Unity 6 기준으로 작업  
마지막 인수인계 업데이트: 2026-06-03

이 문서는 다른 컴퓨터에서 이어서 작업할 수 있도록 현재까지 구현된 게임 프로젝트 진행도만 정리한 것입니다. 과제 제출용 PPT, 체크리스트, 소스코드 제출 자료 내용은 포함하지 않았습니다.

## 이어서 작업하는 방법

1. GitHub 저장소를 다른 컴퓨터에서 clone 하거나 Download ZIP으로 받기
2. Unity Hub에서 프로젝트 폴더 열기
3. Unity가 패키지 임포트 또는 에셋 재임포트를 물으면 진행하기
4. `Assets/Scenes/SampleScene.unity` 열기
5. Play 실행 후 Tank 이동, 발사, Enemy, Target, ItemBox 기능 확인하기

## 현재까지 구현된 주요 기능

### Player Tank

- Tank 이동 기능 유지
- 단발 발사 기능 유지
- 연사 발사 기능 유지
- 플레이어 총알은 `BulletOwner.Player`로 구분됨
- 플레이어 총알은 Target, Enemy, ItemBox를 처리함
- Tank가 자기 총알에 맞아 데미지를 받지 않도록 처리됨

### Tank HP / 피격 / 사망 / Respawn

- Tank 최대 HP: 100
- 시작 HP: 100
- Enemy 총알 데미지: 10
- Enemy 총알에 맞으면 HP UI 즉시 갱신
- 피격 시 hit effect가 있으면 생성되고, Tank 몸체 빨간 점멸 효과 실행
- HP가 0이 되면 `You Die` / `Respawn` UI 표시
- 사망 시 점수 50점 차감
- 사망 후 3초 뒤 시작 위치와 시작 회전으로 Respawn
- Respawn 시 HP 100으로 회복
- Respawn 직후 2초 동안 무적 상태 적용
- 무적 시간 중 Enemy 총알 데미지는 무시됨

### Bullet 처리

- `Bullet.cs`에 `BulletOwner { Player, Enemy }` 구분 있음
- Player 총알은 Target, Enemy, ItemBox 처리용
- Enemy 총알은 Tank 데미지 전용
- Enemy 총알이 빠르게 이동해 Tank를 통과하지 않도록 SphereCast / OverlapSphere 직접 감지 로직 추가
- Bullet 프리팹에 Collider 또는 Rigidbody가 없거나 불안정해도 런타임에서 보정
- 발사자 자신의 Collider는 무시하도록 처리

### Enemy 포탑

- Enemy 포탑은 Tank를 탐지하고, Tank 방향으로 회전하며, 사정거리 안에서 발사함
- Enemy 발사 간격 기본값: 1초
- Enemy 총알 생성 시 `BulletOwner.Enemy`, damage 10으로 설정됨
- Enemy 총알 생성 위치는 포탑 앞쪽으로 약간 보정되어 자기 Collider 안에서 시작하지 않음
- Enemy 포탑 최대 개수: 5개
- Enemy가 파괴되면 Spawner가 최대 5개까지 다시 보충함
- 기존 Enemy 탐지/회전/공격 기능 유지

### Enemy 안전 스폰

- `SampleEnemySpawner.cs`에서 Enemy를 안전 위치에 생성함
- 맵 범위: `arenaMin = (-18, -18)`, `arenaMax = (18, 28)`
- Tank와 너무 가까운 위치 제외
- Enemy끼리 너무 가까운 위치 제외
- Target과 너무 가까운 위치도 가능하면 제외
- 건물, 벽, 장애물 Collider와 겹치거나 너무 가까운 위치 제외
- 스폰 시도 횟수 기본값: 150

### Target 시스템

- Archery Target 개수: 6개
- Dummy Target FREE 개수: 6개
- Archery Target 파괴 시 10점
- Dummy Target FREE 파괴 시 20점
- Target 파괴 시 Score UI 즉시 갱신
- Target 파괴 후 일정 시간 뒤 안전한 위치로 리스폰
- 기존 점수 구조 유지

### Target 이동

- Archery Target도 Dummy Target FREE처럼 자유 이동하도록 변경됨
- Dummy Target FREE는 기존보다 빠르게 자유 이동함
- Archery Target 기본 이동 속도: 2.3
- Dummy Target FREE 기본 이동 속도: 3.5
- `RoamingTarget.cs`가 Archery/Dummy Target 자유 이동 담당
- 이동 전 장애물 검사와 맵 범위 검사를 수행함
- 벽이나 건물을 만나면 이동하지 않고 새 목적지를 선택함
- Target이 특정 구역만 반복하지 않도록 맵 전체에서 목적지를 다시 선택함
- 리스폰 직후 이동 방향과 목적지도 새로 설정됨

### Target 안전 배치 / 안전 리스폰

- `SampleTargetSetBuilder.cs`에서 Archery/Dummy Target을 자동 생성함
- Target 초기 배치 시 `SafeSpawnUtility` 사용
- Target 리스폰 시에도 새 안전 위치를 다시 탐색
- Target끼리 너무 가까이 생성되지 않도록 최소 거리 조건 적용
- Enemy와 너무 가까운 위치도 가능하면 피함
- 벽, 건물, 장애물과 충분히 떨어진 위치만 사용하도록 검사 강화
- 리스폰 직후 주변 장애물이 너무 가까우면 다시 위치 보정 시도

### ItemBox / HealthItem

- ItemBox 자동 생성 기능 추가
- ItemBox 최대 개수: 3개
- ItemBox는 Player 총알로만 파괴 가능
- Enemy 총알은 ItemBox를 파괴하지 않음
- ItemBox 파괴 시 HealthItem 생성
- ItemBox 프리팹이 없어도 기본 Cube fallback으로 동작 가능
- HealthItem 프리팹이 없어도 기본 Sphere fallback으로 동작 가능
- HealthItem 획득 시 Tank HP 20 회복
- HP는 최대 100을 넘지 않음
- HealthItem 획득 반경 기본값: 2
- Trigger 이벤트와 OverlapSphere 보조 검사로 빠르게 지나가도 획득 가능

### Score UI

- `ScoreManager.cs`가 Score UI 자동 생성
- Target 파괴 시 점수 증가
- Tank 사망 시 점수 50점 감소
- 점수는 0 아래로 내려가지 않음

### SafeSpawnUtility

- 공통 안전 스폰 유틸리티 파일: `Assets/scripts/SafeSpawnUtility.cs`
- Enemy, Target, ItemBox 스폰/리스폰에 사용됨
- 주요 검사 내용:
  - 맵 범위 안인지 확인
  - 위에서 아래로 Raycast를 쏴서 바닥 위치 확인
  - 주변 Collider 겹침 검사
  - 벽과의 clearance 검사
  - Tank와의 최소 거리 검사
  - 다른 Target/Enemy와의 최소 거리 검사
  - Trigger Collider 기본 무시 옵션
- 특정 Layer 설정이 없어도 기본 Collider 기준으로 동작하도록 구성됨

### Environment / Map

- EnvLevel 기반 Desert 맵 자동 배치 구조 유지
- `SampleEnvironmentLevelBuilder.cs`가 맵 자동 생성 담당
- Stable Desert Floor 생성 및 Collider 유지
- 건물/장애물에 Collider가 없으면 안전 스폰 검사로 감지할 수 없으므로 경고 로그 유지
- 모든 MeshRenderer에 MeshCollider를 무작정 추가하지 않음

## 주요 스크립트 위치

- `Assets/scripts/SampleTankWeapon.cs`: Tank 발사, HP, 피격, 사망, Respawn, 무적, Heal
- `Assets/scripts/Bullet.cs`: 총알 이동, 충돌, Player/Enemy 총알 구분
- `Assets/scripts/Enemy.cs`: Enemy 포탑 탐지, 회전, 발사, 파괴
- `Assets/scripts/SampleEnemySpawner.cs`: Enemy 5개 자동 생성 및 안전 스폰
- `Assets/scripts/SampleTargetSetBuilder.cs`: Archery/Dummy Target 6개씩 자동 생성
- `Assets/scripts/TargetDestroy.cs`: Target 파괴, 점수, 안전 리스폰
- `Assets/scripts/RoamingTarget.cs`: Archery/Dummy Target 자유 이동 및 장애물 회피
- `Assets/scripts/SafeSpawnUtility.cs`: 공통 안전 스폰/위치 검사
- `Assets/scripts/ScoreManager.cs`: 점수 UI와 점수 증가/감소
- `Assets/scripts/ItemBox.cs`: ItemBox 파괴 및 HealthItem 드롭
- `Assets/scripts/HealthItem.cs`: HP 회복 아이템 획득 처리
- `Assets/scripts/ItemBoxSpawner.cs`: ItemBox 자동 생성 및 재생성
- `Assets/scripts/SampleEnvironmentLevelBuilder.cs`: EnvLevel 맵 자동 배치와 Stable Floor 보정
- `Assets/scripts/VerticalTargetMover.cs`: 기존 Archery 위아래 이동 보조 스크립트. 현재 Archery는 RoamingTarget 자유 이동 사용

## Inspector에서 확인할 주요 값

### SampleTankWeapon

- `maxHp = 100`
- `currentHp = 100`
- `enemyBulletDamage = 10`
- `respawnDelay = 3`
- `invincibleDuration = 2`
- `deathScorePenalty = 50`

### Enemy

- `enemyBulletDamage = 10`
- `fireInterval = 1`
- `bulletSpawnForwardOffset = 0.5`

### SampleEnemySpawner

- `maxEnemies = 5`
- `arenaMin = (-18, -18)`
- `arenaMax = (18, 28)`
- `spawnCheckRadius = 2.2`
- `wallClearanceDistance = 2`
- `minDistanceBetweenEnemies = 9`
- `maxSpawnAttempts = 150`

### SampleTargetSetBuilder

- `archeryTargetCount = 6`
- `dummyTargetCount = 6`
- `archeryMoveSpeed = 2.3`
- `dummyMoveSpeed = 3.5`
- `targetArenaMin = (-18, -18)`
- `targetArenaMax = (18, 28)`
- `archerySpawnCheckRadius = 2.2`
- `dummySpawnCheckRadius = 2.2`
- `wallClearanceDistance = 2`
- `minDistanceBetweenTargets = 6`
- `maxSpawnAttempts = 150`

### HealthItem

- `healAmount = 20`
- `pickupRadius = 2`
- `lifeTime = 15`

### ItemBoxSpawner

- `maxItemBoxes = 3`
- `respawnDelay = 10`
- `healthAmount = 20`
- `minSpawnDistanceFromTank = 6`

## 현재 검증한 내용

- Unity C# 컴파일 성공 로그 확인
- Tank 이동/발사 기능을 건드리지 않는 범위로 수정
- Enemy 총알이 Tank에게 HP 10 데미지를 주는 구조 보강
- HP UI, You Die/Respawn UI, 3초 부활, 2초 무적 구현
- Enemy 5개 안전 스폰 구조 구현
- Archery/Dummy Target 6개씩 자동 생성 구조 구현
- Archery/Dummy Target 자유 이동 및 장애물 회피 구조 구현
- Target 안전 리스폰 구조 구현
- ItemBox/HealthItem 회복 구조 구현
- SafeSpawnUtility 기반 안전 스폰 구조 구현

## 남아 있을 수 있는 한계

- 건물이나 장애물 오브젝트에 Collider가 없으면 SafeSpawnUtility가 감지할 수 없음
- 실제 Play 화면에서 Enemy, Target, ItemBox가 너무 많거나 적게 느껴지면 Inspector 값으로 밸런스 조정 필요
- Target 이동 속도는 맵 구조에 따라 빠르게 느껴질 수 있음
- Enemy 5개는 전투 난이도를 올리므로 HP, ItemBox, Enemy 발사 간격을 함께 보면서 조정 필요
- UI는 기능 중심으로 자동 생성되어 있으므로 발표용으로는 위치, 폰트, 색상 다듬기 가능

## 다음 작업 전 테스트 체크리스트

1. Unity에서 `Assets/Scenes/SampleScene.unity` 열기
2. Play 실행
3. Tank 이동, 단발 발사, 연사 발사 확인
4. Enemy 5개 생성 확인
5. Enemy 발사 간격 1초 확인
6. Enemy 총알 피격 시 HP 100 -> 90 -> 80 감소 확인
7. HP 0 사망, You Die/Respawn UI, 3초 부활 확인
8. Respawn 직후 2초 동안 데미지 무시 확인
9. Archery Target 6개, Dummy Target 6개 생성 확인
10. Archery/Dummy Target이 자유 이동하고 벽에 끼지 않는지 확인
11. Target 파괴 시 Archery 10점, Dummy 20점 유지 확인
12. ItemBox 파괴 후 HealthItem 생성 확인
13. HealthItem 획득 시 HP 20 회복 및 최대 100 제한 확인
14. Console에 컴파일 에러 또는 NullReferenceException이 없는지 확인

## GitHub 참고

- `Library`, `Temp`, `Logs`, `.vs` 폴더는 Unity가 다시 생성하므로 GitHub에 올리지 않아도 됨
- 과제 제출용 PPT, 체크리스트, 제출용 소스 묶음은 이 README에 포함하지 않음
- 실제 Unity 기능 관련 코드는 `Assets/scripts` 기준으로 확인하면 됨
