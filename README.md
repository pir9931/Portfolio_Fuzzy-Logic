# Portfolio_Fuzzy-Logic

중부대학교 게임소프트웨어학과 91913256_김재훈


  
## Introduce

단순히 정해진 시간과 값에 따라 몬스텅의 행동을 구현하는 것이 아닌 유연한 행동을 구현하고자 몬스터의 회피, 공격에 Fuzzy Logic 적용

  

## FuzzyInterfaceSystem.cs

AddRule() : 퍼지 규칙 추가 ( 회피 : 거리, 체력, 회피 | 공격 : 준비, 공격성(플레이어), 공격 )  

Defuzzyfy() : 퍼지 집합의 값을 바탕으로 membership 값과 가중치를 계산해 최종 확률 계산  

GetMembership() : membership 값을 반환   

GetCentroid() : 반환된 멤버십 값의 centroid 값을 계산  

> 입력된 값에 대한 퍼지 집합의 membership 값을 계산하여 퍼지 규칙을 평가하고 행동을 결정함


## Boss.cs

InitializeFuzzySystems() : 회피 및 공격 에 사용될 퍼지 집합, 펴지 규칙   

ShouldAvoid() : 몬스터의 체력과 플레이어와의 거리를 Defuzzify()에 넣어 확률 계산


## Fuzzy-Logic 결과


<table align="center">
  <tr>
    <th style="text-align: center;">FuzzyLogic-Avoid</th>
    <th style="text-align: center;">FuzzyLogic-Attack</th>

  </tr>
  <tr>
    <td><img src="asset/FuzzyAvoid.png" width="300" height="300"></td>
    <td><img src="asset/FuzzyAttack.png" width="300" height="300"></td>

  </tr>
</table>

FuzzyLogic-Avoid  
설정해 놓은 집합 중 가장 높은 가중치를 갖는 값(체력 30% 미만)으로 설정하여 회피하는지 확인  
> Evasion Probability : 0.9 (90% 확률로 회피)

FuzzyLogic-Attack
몬스터가 공격할 준비가 되었는지(쿨타임), 플레이어의 상태 (공격, 이동 중인지)를 계산하여  
attackProbability 값이 Random.value 값보다 높을 경우 패턴 실행
