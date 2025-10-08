# ⚔️ 이터널 리턴 모작 
['이터널 리턴 모작' 프로젝트 상세](https://github.com/HyangRim/Game-Client-Portfoilo/blob/main/DETAIL.md#%EF%B8%8F-%EC%9D%B4%ED%84%B0%EB%84%90-%EB%A6%AC%ED%84%B4-%EB%AA%A8%EC%9E%91-1)

## ➡️NavMesh : 길찾기 알고리즘 
- NavMesh : NavMesh 구축, 경로 찾기 코드
- NavMeshAgent : Movable 오브젝트에 부착하는 컴포넌트, 애니메이션과 FSM변경, 이동 관련 코드 

## 🏁QuadTree : 공간 분할
- QuadTree : 공간 분할 코드, 화면 내부 확인 코드. 
- SceneObjectManager : 오브젝트 관리(추가, 삭제, 충돌, 픽킹) 매니저
- Camera : 렌더링 카메라. 

## 🎥Rendering : 렌더링 
- MeshRenderer : 단순 도형(육면체, 원뿔, 원기둥 등) 렌더러 컴포넌트.
- ModelAnimator : 애니메이션 모델 렌더러 컴포넌트
- ModelRenderer : 모델(애니메이션 제외) 렌더러 컴포넌트
- Renderer : 렌더러 관련 부모 클래스
- RenderManager : 렌더링 파이프라인 총괄(디퍼드 -> 포워드 -> 포스트 프로세싱), 인스턴싱. 

## 🔨Delegate : 델리게이트 코드
- Delegate : 델리게이트 구현 코드

## 🌫️FogOfWar : 전장의 안개 
- 00.DeferredLighting.fx : 디퍼드 렌더링의 라이팅 셰이더. 
- FogOfWar : 전장의 안개 정보 전달 코드(클라이언트)