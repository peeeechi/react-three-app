import * as three from 'three';
import { THREE_AREA_ID } from './constant';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
// import OrbitControls from 'three-orbitcontrols';

export default class ThreeApi {
    constructor(areaID: string) {
        this._areaID = areaID;
    }

    //#region functions ---------------------------


    public init(width: number = 960, height: number = 480): void {

        const canvas =  document.querySelector(`#${this.AreaId}`) as HTMLCanvasElement;

        // レンダラーを作成
        this.renderer = new three.WebGLRenderer({
            canvas: canvas,
        });


        this.renderer.setPixelRatio(window.devicePixelRatio)
        this.renderer.setSize(width, height)

        // シーンを作成
        this.scene = new three.Scene()

        // カメラを作成
        this.camera = new three.PerspectiveCamera(45, width / height)
        this.camera.rotation.y += (90.0 * Math.PI / 180.0);
        this.camera.position.set(0, 0, +1000)
        this.camera.rotation.order = "ZYX";
        
        const helper = new three.AxesHelper(500);
        this.scene.add(helper);

        const size = 500;
        const divisions = 10;

        const gridHelper = new three.GridHelper( size, divisions );
        this.scene.add( gridHelper );

        // 箱を作成
        const geometry = new three.BoxGeometry(400, 50, 1)
        const material = new three.MeshNormalMaterial()
        this.connectorObj = new three.Mesh(geometry, material)
        this.connectorObj.position.set(0, 0, 0);
        this.scene.add(this.connectorObj)

        // const dir = new three.Vector3( 1, 0, 0 );

        // //normalize the direction vector (convert to vector of length 1)
        // dir.normalize();

        // const origin = new three.Vector3( 0, 0, 0 );
        // const length = 100;
        // const arrowColor = 0xffeeff;

        // const arrowHelper = new three.ArrowHelper( dir, origin, length, arrowColor );
        // this.scene.add( arrowHelper );
        
        const controls = new OrbitControls(this.camera, canvas);
        controls.target.set(this.connectorObj.position.x, this.connectorObj.position.y, this.connectorObj.position.z)
        // controls.target.set(arrowHelper.position.x, arrowHelper.position.y, arrowHelper.position.z)
        
        // 滑らかにカメラコントローラーを制御する
        controls.enableDamping = true;
        controls.dampingFactor = 0.2;

        // const onWindowResize = () => {
        //     const cWidth = document.getElementById(THREE_AREA_ID)!.clientWidth;
        //     const cHeight = document.getElementById(THREE_AREA_ID)!.clientHeight;

        //     // レンダラーのサイズを調整する
        //     this.renderer!.setPixelRatio(window.devicePixelRatio);
        //     this.renderer!.setSize( cWidth, cHeight );
                   
        //     // カメラのアスペクト比を正す
        //     this.camera!.aspect = cWidth / cHeight;
        //     this.camera!.updateProjectionMatrix();
        // }

        // const keyEventFunction = (event: KeyboardEvent) => {
        //     const code = event.key;

        //     switch (code) {
        //         case 'x':
        //             // arrowHelper.rotation.x += 0.1;
        //             this.connectorObj!.rotation.x += 0.1;
        //             break;
        //         case 'y':
        //             // arrowHelper.rotation.y += 0.1;
        //             this.connectorObj!.rotation.y += 0.1;
        //             break;
        //         case 'z':
        //             // arrowHelper.rotation.z += 0.1;
        //             this.connectorObj!.rotation.z += 0.1;
        //             break;
            
        //         default:
        //             break;
        //     }

        // }
        // window.addEventListener('resize', onWindowResize, false);
        window.addEventListener('resize', this.onWindowResize, false);
        window.addEventListener('keydown', this.keyEventFunction);


        // 毎フレーム時に実行されるループイベントです
        const tick = () => {

            // controls.update();
            // box.rotation.y += 0.01
            this.renderer!.render(this.scene!, this.camera!) // レンダリング

            requestAnimationFrame(tick)
        };

        tick();
    }

    //#endregion

    //#region Props -------------------------------

    private _areaID: string;
    public get AreaId(): string {
        return this._areaID;
    }

    public scene: three.Scene|null = null;
    public camera: three.PerspectiveCamera|null = null;
    public renderer: three.WebGLRenderer|null = null;
    public connectorObj: three.Mesh| null = null;

    public onWindowResize() {
        const cWidth = document.getElementById(THREE_AREA_ID)!.clientWidth;
        const cHeight = document.getElementById(THREE_AREA_ID)!.clientHeight;

        // レンダラーのサイズを調整する
        this.renderer!.setPixelRatio(window.devicePixelRatio);
        this.renderer!.setSize( cWidth, cHeight );
                
        // カメラのアスペクト比を正す
        this.camera!.aspect = cWidth / cHeight;
        this.camera!.updateProjectionMatrix();
    }

    public keyEventFunction = (event: KeyboardEvent) => {
        const code = event.key;

        switch (code) {
            case 'x':
                // arrowHelper.rotation.x += 0.1;
                this.connectorObj!.rotation.x += 0.1;
                break;
            case 'y':
                // arrowHelper.rotation.y += 0.1;
                this.connectorObj!.rotation.y += 0.1;
                break;
            case 'z':
                // arrowHelper.rotation.z += 0.1;
                this.connectorObj!.rotation.z += 0.1;
                break;
        
            default:
                break;
        }

    }

    //#endregion ----------------------------------
}