import * as three from 'three';
import { THREE_AREA_ID } from './constant';
// import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
// import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { TrackballControls } from 'three/examples/jsm/controls/TrackballControls';
import { GLTFLoader, GLTF } from 'three/examples/jsm/loaders/GLTFLoader';

// import 'three/GLTFLoader';
// import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader';

export type ObjectIntersectEvent = (obj: three.Intersection[]) => void;

export const loadGLTF = async (filepath: string) => {

    return new Promise<GLTF>((resolve, error: (err: ErrorEvent) => void) => {

        const gltfLoader = new GLTFLoader();

        const onload = (gltf: GLTF) => {
            resolve(gltf);
        }

        const onProgress = (progress: ProgressEvent<EventTarget>) => {
            console.log(progress.lengthComputable);
        }

        const onError = (err: ErrorEvent) => {
            error(err);
        }

        gltfLoader.load(filepath, onload, onProgress, onError);
    });
}

export default class ThreeApi {
    constructor(areaID: string, background?: three.Color|three.Texture|three.WebGLCubeRenderTarget) {
        this._areaID = areaID;

        // シーンを作成
        this.scene = new three.Scene();

        if (background != null)
            this.scene.background = background;
    }

    //#region functions ---------------------------

    /**
     * 初期化処理
     */
    public init(): void {

        const canvas =  document.querySelector(`#${this.areaId}`) as HTMLCanvasElement;

        // レンダラーを作成
        this.renderer = new three.WebGLRenderer({
            canvas: canvas,
        });


        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.setSize(canvas.clientWidth, canvas.clientHeight);

        // シーンを作成
        this.scene = new three.Scene();
        this.scene.background = new three.Color(0x111111);

        // カメラを作成
        this.camera = new three.PerspectiveCamera(45, canvas.clientWidth / canvas.clientHeight)
        this.camera.rotateY(90.0 * Math.PI / 180.0);
        this.camera.position.set(5, -10, 5);
        this.camera.lookAt(0,0,0);
        // this.camera.rotation.order = "ZYX";

        const createHelper = (helperX:number, helperY:number, helperZ:number, length: number) => {

            const mHarfLength = length / -2.0;
            const headSize = length * 0.02;

            const xArrow = new three.ArrowHelper(new three.Vector3(1, 0, 0), new three.Vector3(mHarfLength, helperY, helperZ), length, "red", headSize, headSize);
            const yArrow = new three.ArrowHelper(new three.Vector3(0, 1, 0), new three.Vector3(helperX, mHarfLength, helperZ), length, "green", headSize, headSize);
            const zArrow = new three.ArrowHelper(new three.Vector3(0, 0, 1), new three.Vector3(helperX, helperY, mHarfLength), length, "blue", headSize, headSize);

            const axis = new three.Group();

            axis.add(xArrow);
            axis.add(yArrow);
            axis.add(zArrow);

            return axis;
        }

        const axis = createHelper(0, 0, 0, 10);
        this.scene.add(axis);

        const createGrid = (size: number, divisions: number) => {
            return new three.GridHelper( size, divisions, 0x777777, 0x333333);
        }

        const xyGrid = createGrid(10, 10);
        const xzGrid = createGrid(10, 10);
        const yzGrid = createGrid(10, 10);

        xyGrid.position.y += 5;

        xzGrid.rotation.z += Math.PI * 90 / 180;
        xzGrid.position.x -= 5;

        yzGrid.rotation.x += Math.PI * 90 / 180;
        yzGrid.position.z -= 5;

        this.scene.add(xyGrid);
        this.scene.add(xzGrid);
        this.scene.add(yzGrid);




        // this.scene.add( gridHelper );
        // const harf = 5;
        // const gridtick = harf/10;
        // const grid = new three.Group();
        // const gridM = new three.LineBasicMaterial({color: 0x005555, opacity: 0.5, transparent: true});

        // const addLine = (p1: three.Vector3, p2: three.Vector3) => {
        //     const line = new three.Line( new three.BufferGeometry().setFromPoints([p1, p2]), gridM);
        //     grid.add( line );

        // }

        // const addMesh = (mesh: three.Object3D) => {
        //     this.scene?.add(mesh);
        // }

        // const p = new Promise<void>(resolve => {

        //     for (let x = -harf; x < harf; x+=gridtick) {
        //         addLine(new three.Vector3(x, -harf, -harf), new three.Vector3(x, harf, -harf));
        //         addLine(new three.Vector3(x, -harf, -harf), new three.Vector3(x, -harf, harf));

        //         for (let y = -harf; y < harf; y+=gridtick) {
        //             addLine(new three.Vector3(-harf, y, -harf), new three.Vector3(-harf, y, harf));
        //             addLine(new three.Vector3(-harf, y, -harf), new three.Vector3(harf, y, -harf));
        //             for (let z = -harf; z < harf; z+=gridtick) {
        //                 addLine(new three.Vector3(-harf, -harf, z), new three.Vector3(-harf, harf, z));
        //                 addLine(new three.Vector3(-harf, -harf, z), new three.Vector3(harf, -harf, z));

        //             }
        //         }
        //     }

        //     grid.position.set(0,0,0);
        //     addMesh(grid);

        //     resolve();
        // })


        const light = new three.AmbientLight(0xffffff, 5);

        light.position.set(0, 10, 0);
        light.castShadow = true;

        this.scene.add(light);

        canvas.addEventListener('mousemove', this.handleMouseMove);
        window.addEventListener('resize', this.onWindowResize);
        // window.addEventListener('keydown', this.keyEventFunction);


        this.raycaster = new three.Raycaster();

        // 毎フレーム時に実行されるループイベントです
        const tick = () => {

            // controls.update();
            if (this.scene && this.camera && this.renderer && this.raycaster) {

                this.orbit?.update();
                this.trackball?.update();

                this.renderer.render(this.scene, this.camera) // レンダリング
                this.raycaster.setFromCamera(this.mousePos, this.camera);

                // その光線とぶつかったオブジェクトを得る
                const intersects = this.raycaster.intersectObjects(this.scene.children);

                if(intersects && intersects.length > 0 && this.ObjectIntersected.length > 0) {
                    this.ObjectIntersected.forEach(event => {
                        event(intersects);
                    });
                }

                requestAnimationFrame(tick)
            }
        };

        tick();
    }

    /**
     * 画面サイズに合わせて描画エリアをリサイズする
     *
     * @private
     * @memberof ThreeApi
     */
    private onWindowResize = () => {
        const cWidth = document.getElementById(THREE_AREA_ID)!.clientWidth;
        const cHeight = document.getElementById(THREE_AREA_ID)!.clientHeight;

        // レンダラーのサイズを調整する
        this.renderer!.setPixelRatio(window.devicePixelRatio);
        this.renderer!.setSize( cWidth, cHeight );

        // カメラのアスペクト比を正す
        this.camera!.aspect = cWidth / cHeight;
        this.camera!.updateProjectionMatrix();
    }

    // private keyEventFunction = (event: KeyboardEvent) => {
    //     const code = event.key;

    //     switch (code) {
    //         case 'x':
    //             // arrowHelper.rotation.x += 0.1;
    //             this.camera!.rotateX(0.1);
    //             break;
    //         case 'y':
    //             // arrowHelper.rotation.y += 0.1;
    //             this.camera!.rotateY(0.1);
    //             break;
    //         case 'z':
    //             // arrowHelper.rotation.z += 0.1;
    //             this.camera!.rotateZ(0.1);
    //             break;

    //         default:
    //             break;
    //     }

    // }

    /**
     * マウス座標追跡処理(Raycasterに使用)
     *
     * @private
     * @param {MouseEvent} event
     * @memberof ThreeApi
     */
    private handleMouseMove = (event: MouseEvent) => {
        const element = event.currentTarget as HTMLCanvasElement;
            // canvas要素上のXY座標
            const x = event.clientX - element.offsetLeft;
            const y = event.clientY - element.offsetTop;

            // canvas要素の幅・高さ
            const w = element.offsetWidth;
            const h = element.offsetHeight;

            // -1〜+1の範囲で現在のマウス座標を登録する
            this.mousePos.x = ( x / w ) * 2 - 1;
            this.mousePos.y = -( y / h ) * 2 + 1;
    }


    public setOrbit = (target: three.Vector3) => {
        if ( this.renderer != null && this.camera != null) {

            if (this.trackball != null)
                this.disposeTrackball();

            if (this.orbit == null ) {
                this.orbit = new OrbitControls(this.camera, this.renderer.domElement);
                this.orbit.enableDamping = true;
                this.orbit.dampingFactor = 0.2;
            }

            this.orbit.target = target;
        }
    }

    public setTrackball = (target: three.Vector3) => {
        if (this.renderer != null && this.camera != null) {

            if (this.orbit != null)
                this.disposeOrbit();

            if (this.trackball == null) {
                this.trackball = new TrackballControls(this.camera, this.renderer.domElement);
                this.trackball.rotateSpeed = 5.0; //回転速度
                this.trackball.noRoll = true;
                this.trackball.zoomSpeed = 0.5;//ズーム速度
                this.trackball.panSpeed = 0.5;//パン速度
                console.log("set trackball");
            }
            
            this.trackball.target = target;
            console.log("set trackball target:", target);
        }
    }

    public disposeTrackball = () => {
        if (this.trackball != null) {
            this.trackball.dispose();
            this.trackball = null;
        }
    }

    public disposeOrbit = () => {
        if (this.orbit != null) {
            this.orbit.dispose();
            this.orbit = null;
        }
    }

    public addObject = (obj: three.Object3D) => {

        return new Promise(resolve => {

            const isSame = this.scene.children.find(v => {
                v.id === obj.id
            });

            if (isSame == null)
                this.scene.add(obj);

            resolve();
        });
    }

    public removeObject = (obj: three.Object3D) => {
        return new Promise(resolve => {
            this.scene.remove(obj);
            resolve();
        });
    }


    //#endregion

    //#region Props -------------------------------
    private _areaID: string;
    public get areaId(): string {
        return this._areaID;
    }

    public scene: three.Scene;
    public camera: three.PerspectiveCamera|null = null;
    public renderer: three.WebGLRenderer|null = null;
    public raycaster: three.Raycaster| null = null;

    private _mousePos: three.Vector2 = new three.Vector2(0,0);
    public get mousePos(): three.Vector2 {
        return this._mousePos;
    }

    public ObjectIntersected: ObjectIntersectEvent[] = [];

    // private canvas: HTMLCanvasElement | null = null;
    private trackball: TrackballControls|null = null;
    private orbit: OrbitControls|null = null;




    //#endregion ----------------------------------
}