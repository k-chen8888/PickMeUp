using UnityEngine;
using System.Collections;

/* This is a sample animation logic implementation
 * Works with Unity-chan
 * The NavMesh controller expects a C# script called "AnimateLogic", otherwise, it will ignore animations
 */
public class AnimateLogic : MonoBehaviour {
    public static AnimateLogic Static;

    private Animator anim;
    private AnimatorStateInfo currentBaseState;
    private UnityChanControlScriptWithRgidBody settings;

    private CapsuleCollider col;
    private Rigidbody rb;
    private Vector3 velocity;
    private float orgColHight;
    private Vector3 orgVectColCenter;

    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int locoState = Animator.StringToHash("Base Layer.Locomotion");
    static int jumpState = Animator.StringToHash("Base Layer.Jump");
    static int restState = Animator.StringToHash("Base Layer.Rest");

    private bool reset = false;
    private bool jump = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        settings = GetComponent<UnityChanControlScriptWithRgidBody>();
        
        col = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        Static = this;
    }

    public void AnimateNavMesh(bool moving, bool jumping)
    {
        if (moving)
        {
            jump = jumping;
            Animate(0.0f, 0.5f);
            reset = true;
        }
        else if (reset)
        {
            Animate(0.0f, 0.0f);
            reset = false;
        }
    }

    private void Animate(float h, float v)
    {
        anim.SetFloat("Speed", v);                          // Animator側で設定している"Speed"パラメタにvを渡す
        anim.SetFloat("Direction", h);                      // Animator側で設定している"Direction"パラメタにhを渡す
        anim.speed = settings.animSpeed;                             // Animatorのモーション再生速度に animSpeedを設定する
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0); // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
        rb.useGravity = true;//ジャンプ中に重力を切るので、それ以外は重力の影響を受けるようにする



        // 以下、キャラクターの移動処理
        velocity = new Vector3(0, 0, v);        // 上下のキー入力からZ軸方向の移動量を取得
                                                         // キャラクターのローカル空間での方向に変換
        velocity = transform.TransformDirection(velocity);
        //以下のvの閾値は、Mecanim側のトランジションと一緒に調整する
        if (v > 0.1)
        {
            velocity *= settings.forwardSpeed;       // 移動速度を掛ける
        }
        else if (v < -0.1)
        {
            velocity *= settings.backwardSpeed;  // 移動速度を掛ける
        }

        if (jump)
        {   // スペースキーを入力したら

            //アニメーションのステートがLocomotionの最中のみジャンプできる
            if (currentBaseState.fullPathHash == locoState)
            {
                //ステート遷移中でなかったらジャンプできる
                if (!anim.IsInTransition(0))
                {
                    rb.AddForce(Vector3.up * settings.jumpPower, ForceMode.VelocityChange);
                    anim.SetBool("Jump", true);     // Animatorにジャンプに切り替えるフラグを送る
                }
            }

            jump = false;
        }


        // 上下のキー入力でキャラクターを移動させる
        transform.localPosition += velocity * Time.fixedDeltaTime;

        // 左右のキー入力でキャラクタをY軸で旋回させる
        if (h > 0)
            transform.Rotate(0, h * settings.rotateSpeed, 0);


        // 以下、Animatorの各ステート中での処理
        // Locomotion中
        // 現在のベースレイヤーがlocoStateの時
        if (currentBaseState.fullPathHash == locoState)
        {
            //カーブでコライダ調整をしている時は、念のためにリセットする
            if (settings.useCurves)
            {
                resetCollider();
            }
        }
        // JUMP中の処理
        // 現在のベースレイヤーがjumpStateの時
        else if (currentBaseState.fullPathHash == jumpState)
        {
            if (!anim.IsInTransition(0))
            {

                // 以下、カーブ調整をする場合の処理
                if (settings.useCurves)
                {
                    // 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
                    // JumpHeight:JUMP00でのジャンプの高さ（0〜1）
                    // GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
                    float jumpHeight = anim.GetFloat("JumpHeight");
                    float gravityControl = anim.GetFloat("GravityControl");
                    if (gravityControl > 0)
                        rb.useGravity = false;  //ジャンプ中の重力の影響を切る

                    // レイキャストをキャラクターのセンターから落とす
                    Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
                    RaycastHit hitInfo = new RaycastHit();
                    // 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        if (hitInfo.distance > settings.useCurvesHeight)
                        {
                            col.height = orgColHight - jumpHeight;          // 調整されたコライダーの高さ
                            float adjCenterY = orgVectColCenter.y + jumpHeight;
                            col.center = new Vector3(0, adjCenterY, 0); // 調整されたコライダーのセンター
                        }
                        else {
                            // 閾値よりも低い時には初期値に戻す（念のため）					
                            resetCollider();
                        }
                    }
                }
                // Jump bool値をリセットする（ループしないようにする）				
                anim.SetBool("Jump", false);
            }
        }
        // IDLE中の処理
        // 現在のベースレイヤーがidleStateの時
        else if (currentBaseState.fullPathHash == idleState)
        {
            //カーブでコライダ調整をしている時は、念のためにリセットする
            if (settings.useCurves)
            {
                resetCollider();
            }
            // スペースキーを入力したらRest状態になる
            if (Input.GetButtonDown("Jump"))
            {
                anim.SetBool("Rest", true);
            }
        }
        // REST中の処理
        // 現在のベースレイヤーがrestStateの時
        else if (currentBaseState.fullPathHash == restState)
        {
            //cameraObject.SendMessage("setCameraPositionFrontView");		// カメラを正面に切り替える
            // ステートが遷移中でない場合、Rest bool値をリセットする（ループしないようにする）
            if (!anim.IsInTransition(0))
            {
                anim.SetBool("Rest", false);
            }
        }
    }

    void resetCollider()
    {
        // コンポーネントのHeight、Centerの初期値を戻す
        col.height = orgColHight;
        col.center = orgVectColCenter;
    }
}
