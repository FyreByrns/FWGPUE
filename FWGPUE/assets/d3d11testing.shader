struct vs_in {
    float3 position_local : POS;
    matrix transform : TRANSFORM;
};
struct vs_out {
    float4 position_clip : SV_POSITION;
};
vs_out vs_main(vs_in input) {
    vs_out output = (vs_out)0;
    output.position_clip = float4(input.position_local, 1.0);
    return output;
}
float4 ps_main(vs_out input) : SV_TARGET{
    return float4(1.0, 0.5, 0.2, 1.0);
}