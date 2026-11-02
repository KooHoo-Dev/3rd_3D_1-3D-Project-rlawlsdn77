namespace OverFixed
{
    // 경로 지대 효과와 행성 정비 혜택의 종류 (ZoneTable EffectType, PlanetTypeTable MaintenanceEffectType)
    // Rate는 원래 값에 곱하는 배율, Discount는 할인율 (0.3 = 30% 할인, 여러 할인은 더해서 적용)
    public enum EffectType
    {
        None,
        SpawnRate,                  // 재료 전체 생성량 배율
        ScrapRate,                  // 고철 생성량 배율
        CrystalRate,                // 크리스탈 생성량 배율
        FlowSpeedRate,              // 재료 흐름 속도 배율
        AccidentRate,               // 패널 사고 빈도 배율 (사고 타이머가 흐르는 속도, 0이면 사고 없음)
        MiniGameZoneRate,           // 타이밍 바 성공 구간 폭 배율
        OrderRewardRate,            // 그 구간에 생긴 주문의 보상 배율
        OrderTimeRate,              // 그 구간에 생긴 주문의 제한시간 배율
        CapsuleSpawn,               // 구조 캡슐 초당 생성 수
        CapsuleReward,              // 구조 캡슐 하나의 보상 (크레딧)
        StabilizerDiscount,         // 안정화 물질 할인율
        NextMaintenanceDiscount,    // 다음 정비 상점 할인율
        RepairDiscount,             // 모듈 복구·재건 할인율
        ModuleDiscount,             // 모듈 구매 할인율
        RefundRate,                 // 모듈 철거 환불 비율 (구매가에 곱함)
    }
}
