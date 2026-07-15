namespace DS.IHF.Services;

public class DsEnvironment
{
    public string Name { get; }
    public string AdminUserGuid { get; }
    public string IntegrationKey { get; }
    public string AccountId { get; }
    public string PrivateKeyPem { get; }
    public string BaseUri { get; }
    public string AuthServer { get; }

    private DsEnvironment(string name, string adminGuid, string integrationKey,
        string accountId, string privateKeyPem, string baseUri, string authServer)
    {
        Name = name;
        AdminUserGuid = adminGuid;
        IntegrationKey = integrationKey;
        AccountId = accountId;
        PrivateKeyPem = privateKeyPem;
        BaseUri = baseUri;
        AuthServer = authServer;
    }

    // ─── Produktionsumgebung ──────────────────────────────────────────────────
    public static readonly DsEnvironment Production = new(
        name: "Produktion",
        adminGuid: "75e53d4c-1ef7-41aa-8f40-b0e154a2c051",
        accountId: "2fd6a979-c585-4a54-bac2-4ead62bf4ab8",
        integrationKey: "f5188d5c-d9f3-4523-b610-bd54417d5a86",
        privateKeyPem: """
            -----BEGIN RSA PRIVATE KEY-----
            MIIEogIBAAKCAQEAvBGd+n4iPePk3VGKyO34vKn4/9TCU4JzcDs2YpM3Ealzomki
            0Z/FWxjrGoAEbBnxcigxqerCHw1k8BguXIleZaPAh1axeVWxnG2d2RebexXrQd03
            ZjL9sxdmd/P1NQ+6zS+lk/PUcsXIk2gHQuGa9yZNQA00KE+vgPxNloghhENt9GoQ
            Y16/qpI2IVK8Q62O+Lu8EzHQAknkK6qWrcdrywlDa2n8ma6v9Mi+bmL13mpqW+kO
            aACNSMuBPwJlbvraC8RqnhMemDXlEp2MTAwQzMVySIr5ztgsTdSM7YdQpG2Lv4Fw
            NulIkUsLGJMvK/OUiLNKnp2V0RiKv5LkfpJHAQIDAQABAoH/FH1G3NeJG30MniAY
            IrPqeYN7IUQot7hqozuQPUUUptPzSSpzJKtncPlSA90WnkBlUa6XWo+8/m0TJiU8
            R9CffCtO+m+zfiib5RL8M36i7dIBg3d6ZaQAh9Zwz3jAqjtAesJKI8hYyPhLVePr
            hpnVrD98AxJtMEceM8tJGLFUFXijWT/XieanwiILvwS6bBLp6WWLlim85T+O/6Cj
            OWSXGS/t1OWcQ2nKh7M+nUA8mf35r4DcAfkyDBVj4WFLsqJdn13ktHRJ5IaIROYI
            OmnsRnVmwMEl/boeOv2JRLYGLJlaOdShM8/dbUqLjZXyw9z29sLMCGK05KvZ6XdR
            XpkBAoGBAOHvEIdDDD6HRXb9qrExnEAsjBPvRotxf/tprZDQaeGKYHEOhwRRAf+G
            7zGdxjSrEuKTYNEWDOQJ5JbVQVsOceh9nF3kytD5fuLzfI11dz9wjBZct0l3w+80
            hq2Soprp7Yv0S4rN3GuGfk2xLrbYuaXd1eZx4UdwOGStTbCvud2BAoGBANUYmMEj
            1DzXzC1jmoKm4w5kXxe6vyEI0BE7C7FRrvvGgLM2LxU3rXrhhiQ+8QSG3KIefcmf
            yMdRR5SM2WIB8ut4Ox8j8wd1rqld3PmowQmQ8QiEtp69eaLiq6h9Mg1DMZePBCo8
            H1HiyY3tk8kBDs5D0/gRPqBvejrR1PLFcCmBAoGAHjFKbikBm+GL4OjpRKCylsjd
            N1TEgqH6TmjC7xVK8P8DAFjGpkcFE1a+5EyHTkaGUY0MZSSjOF4yFA4Pm9GEW2Nd
            4BZRHDgbQszzGhxWgT3TGrHtNH4yyuakENIFtNoKCqfs6HG2QRBKFKvW6ExyEr5g
            dVlGl7thbeLS/QeeuQECgYEAl2/pbqh1td9uHHuCXIMZLSsrYQO3vFQ0+WnKv3Lb
            NdY1tCY3g46T3JXU7IFGav8kYJnmrpi86NjcU7dc7QeRiMFi0piLp6t8OqSX21yQ
            AqpcgL8/wMPKY3VOpGiEX2R3I8vhG9qqL1lJ/3Ds65Wy3ebaBprKtN1EMfTYrsad
            FQECgYEAu8QfzpGuus2iofoPEShWKamKMo66FoiGjeiOVUOmoZQViE2wGAFS68NF
            bbEoX4ZyR6b4J+mKYoKy7snoub4Z+GsOGehzhuYoKKUULKOSTdNgEFwgYaNriHYM
            mWV2WgPGrMMCppCeRNOoR27pHoWKl9X1vV1PrXfksN44iQSPPAk=
            -----END RSA PRIVATE KEY-----
            """,
        baseUri: "https://eu.docusign.net/restapi",
        authServer: "https://account.docusign.com"
    );

    // ─── Demoumgebung ─────────────────────────────────────────────────────────
    public static readonly DsEnvironment Demo = new(
        name: "Demo",
        adminGuid: "3a32b6d7-c767-4de2-a8b7-f8431613cdfe",
        integrationKey: "f5188d5c-d9f3-4523-b610-bd54417d5a86",
        accountId: "e74d0308-f069-492b-92b8-0c43ac20c5ec",
        privateKeyPem: """
            -----BEGIN RSA PRIVATE KEY-----
            MIIEowIBAAKCAQEAgAv76N+gKk/zxazNhEsXDcgvnYWf5XGWGbX8YxK4a1XfiBMv
            INIBPs22PX7RCJEvfxh9Bb+liQTEb+IqGvKp8P62/nI8jQHInmMFH6NTyRoG/zDR
            8uvB2lER6uMIAhta4Mp5wAJjtxeErqYknPYxWkA3CdhlOsxFVst36oM7oI7KUGXo
            kdsZfhZIxUTnezpA+kcWvV0AOtsx0tYhT9luY2gVam6P8BZES3CLpFFSlL2Uj4HJ
            j4G+FHxwgKHydA3VHgD0ERqKd5QBzPHK5ieyhx0fIkC1/NlSo10v9QFWdVVOqQAr
            8zSYR9YumiEY1bmd/K5YJls9W/6G90FQhzEmcQIDAQABAoIBAA4Cwa6qgXj/xJm1
            nctC+CPO6r7ety9A7X6kthgVHU7JV6spBjgeInq/wZga/z5jvIG4qT1uyesjNWdU
            pOzL7qJrDJTs5Qk8z7nc9duPYISZr3hO3DorZ8u+oSpFa0PnwFrmbMoAuO54yVDe
            5xSxr5bDfgI6xmDXpyKnItjrQSQBMq5Kg0kQMTfog/HiCPC4sqVX8skmPy/Fkepf
            shBv5drD7W7yqDonk8HQuuBYwX8XvGJ4iA2rrENDkMljnVVQITNoUi4GT+qchk8K
            bADY/Sidtx6aWsNGAMDZ74a4A7eG+h2Dyf69YrYHZjtYikTt0X2Dz3FZaTW3J+gw
            lIki51ECgYEA7hFsrB9yBpVs1j13XBHyYKCH2fKnpo2+JBreWo+JBFhMO6YUaVRS
            dPAPp4jjySD3xKQIxWFykLf7hOViZAfHi+n6xfQmn6hz8DE+ZPv0EMA4p0i+FDPt
            hKeudLlUCz0l5FjHTtx0KMH6JRFLC+0ZPv1oKVw53G9Q+DN90MQafG0CgYEAibEP
            0C+ZlEeYkRxQGneb72oKWCJQPN2+o9YcrIHpu4V1Tl2SDLuv5JWD7FC0VZYkk7AJ
            S1txY3K296OEJQqDYofT29l2pnu3QiOeJ8nSmdR/l1xd/cubX3+1twht1y1hFflV
            VWB1DufZq/nC5MQRM6VxoWD3IFcDZHLZ/9yGx5UCgYEA5TNxZOWBakVCW6Eh4UqG
            B/GF2Zd9QSUGAsy7doTqbXx1KJhk3mOIX30gFTP92g4bfP5QU3fWTO5VAUW2zIeQ
            3muOPDcrWFteA8nlQGQyPk5SPPwTxG+aJTUrCMXE3G0qpgWzHYGxc+wNYNKdZgYH
            YFWoiul363pgghVQ9EVZGqECgYBspdOFgdncwDXp4v5uNA1OeE0bSFAqBwtD+lJ7
            6LMHLgLnSDxTYdIkO7pyQShbFHNeOhzLYqdxQnaPp25BUC9mEymgI9NVAPAU90f3
            u3A7xAq947ui5QN/8qTvfW42yW7/SNQF32vezCHdauJXY0Lzjsu//GRkF2Ts4ReV
            pmyPlQKBgGVG07+sRLQVh0jCRy41W33BV57KH0GfZVUL8JP1TDN7IPT0D6WmmpFj
            rUYRNwyClkHKzb0s+F1ZCtyXn/pUOrlpmISoUv761q6a+jognpQDNKEvveTp57SF
            3780azgC6JvkMOcSr64SP8O3TdtvToS7hXbWE9Ci3RrTIbOfC9jb
            -----END RSA PRIVATE KEY-----            
            """,
        baseUri: "https://demo.docusign.net/restapi",
        authServer: "https://account-d.docusign.com"
    );

    // Aktive Umgebung — wird beim Programmstart gesetzt
    public static DsEnvironment Active { get; private set; } = Production;

    public static void SetDemo() => Active = Demo;
}
