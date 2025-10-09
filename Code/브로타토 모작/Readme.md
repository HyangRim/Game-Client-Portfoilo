# 🕹️ Brotato 모작
['브로타토 모작' 프로젝트 상세](https://github.com/HyangRim/Game-Client-Portfoilo/blob/main/DETAIL.md#%EF%B8%8F-brotato-%EB%AA%A8%EC%9E%91)

## 🔨Manager : 매니저 코드
- CCollisionMgr : 충돌 처리 코드
- CEventMgr : 이벤트(생성, 삭제, Scene 변경등) 처리 매니저
- CFileMgr : 파일 로딩 매니저 -> 파일 형식에 따라 올바른 매니저(사운드, 이미지등)에 분배.  
- CKeyMgr : 키 입력 매니저
- CResMgr : 텍스쳐 관련 리소스 매니저
- CSoundMgr : 사운드(BGM, SFX) 매니저
- Direct2DMgr : 그래픽 렌더링 관련 매니저. 

## 🌫️Object : 오브젝트(캐릭터, 몬스터, 무기) 코드
- CMobSpawner : 웨이브에 맞춰 몬스터를 랜덤하게 스폰하는 매니저. 
- CMonFactory : 몬스터 혹은 골드를 생성하는 팩토리.
- CMonster : 몬스터 부모 클래스
- CNormal_Monster : 근접 일반 몬스터 
- CPlayer : 플레이어 클래스. 
- CWeapon : 무기 부모 클래스. 