import React, {useEffect, useState} from 'react';
import * as THREE from 'three';
// import {TrackballControls} from 'three/examples/jsm/controls/TrackballControls';
// import Stats from "three/examples/jsm/libs/stats.module";


type PlotData = {
    name: string;
    data: number;
}

type RealTimeChartThreeProps = {
    canvasId: string;
    watchList: PlotData[];
    rangeX: number;
    rangeY: number;
    displayCount: number;
}

type ChartState = {
    array: Float32Array;
    mesh: THREE.Line<THREE.BufferGeometry, THREE.LineBasicMaterial>;
    count: number;
    index: number;
}


export default class RealTimeChartThree extends React.Component<RealTimeChartThreeProps> {

    constructor (props: RealTimeChartThreeProps) {

        super(props);
    }

    private chartStateList: ChartState[] = []
    private scene: THREE.Scene|null = null;
    private camera: THREE.PerspectiveCamera|null = null;
    private renderer: THREE.WebGLRenderer| null = null;
    private axisX: THREE.ArrowHelper| null = null;
    private axisY: THREE.ArrowHelper| null = null;
    private det: number = 0;   
    private loopId: number = 0;
    
    private init = () => {
    
        const canvas = document.getElementById(this.props.canvasId) as HTMLCanvasElement;
    
        if (canvas != null) {
    
            // setScene(new THREE.Scene());
            this.scene = new THREE.Scene();
    
            // setCamera(new THREE.PerspectiveCamera(
            //     75,
            //     canvas.clientWidth / canvas.clientHeight,
            //     0.1,
            //     1000)
            // );
            this.camera = new THREE.PerspectiveCamera(
                75,
                canvas.clientWidth / canvas.clientHeight,
                0.1,
                1000
            );
    
            // setRenderer(new THREE.WebGLRenderer({
            //     canvas: canvas
            // }));
    
            this.renderer = new THREE.WebGLRenderer({
                canvas: canvas
            });
    
    
            this.renderer?.setPixelRatio(window.devicePixelRatio);
            this.renderer?.setSize(canvas.clientWidth, canvas.clientHeight);
    
            // trackball = new TrackballControls(camera, renderer.domElement);
    
            // trackball.rotateSpeed = 5.0; //回転速度
            // trackball.noRoll = true;
            // trackball.zoomSpeed = 0.5; //ズーム速度
            // trackball.panSpeed = 0.5; //パン速度
    
            // setAxisX(new THREE.ArrowHelper(
            //     new THREE.Vector3(1, 0, 0),
            //     new THREE.Vector3(rangeX / -2, 0, 0),
            //     rangeX,
            //     "#ff0000",
            //     0.2,
            //     0.2
            //   )
            // );
            this.axisX = new THREE.ArrowHelper(
                new THREE.Vector3(1, 0, 0),
                new THREE.Vector3(this.props.rangeX / -2, 0, 0),
                this.props.rangeX,
                "#ff0000",
                0.2,
                0.2
              )
    
    
            // setAxisY(new THREE.ArrowHelper(
            //     new THREE.Vector3(0, 1, 0),
            //     new THREE.Vector3(rangeX / -2, -rangeY, 0),
            //     rangeY * 2,
            //     "#00ff00",
            //     0.2,
            //     0.2)
            //     );
            this.axisY = new THREE.ArrowHelper(
                new THREE.Vector3(0, 1, 0),
                new THREE.Vector3(this.props.rangeX / -2, this.props.rangeY / -2, 0),
                this.props.rangeY,
                "#00ff00",
                0.2,
                0.2
            );
    
    
            if(this.scene != null) {
                if (this.axisX != null) this.scene.add(this.axisX);
                if (this.axisY != null) this.scene.add(this.axisY);
            }
    
    
            // setDet(rangeX / displayCount);
            this.det = this.props.rangeX / this.props.displayCount;
            if (this.camera != null) {
                this.camera.position.z = this.props.rangeX * 1.0;
            }
    
            const colorList = [
                "red",
                "green",
                "blue",
            ];

            const copy = JSON.parse(JSON.stringify(this.props.watchList)) as PlotData[];
    
    
            for (let i= 0; i < copy.length; i++) {
                this.addChart(colorList[i % colorList.length], copy[i].name);
            }
    
            // window.setInterval(() => {
            //     // setArray(Math.random(), 0);
            //     // console.log("update: ", getArray(0));
            //     console.log("update: ", getFunc(0));
            //     updateChart();
            // }, 10);
    
            // stats.showPanel(0);
            // div!.appendChild(stats.dom);
    
            this.animate();
        }
    
    
    }
    
    private addChart = (color: string|number|THREE.Color, name: string) => {
    
        if (this.scene != null) {
    
            // const copyArray = JSON.parse(JSON.stringify(chartStateList));
    
            const geometry  = new THREE.BufferGeometry();
            const positions = new Float32Array(this.props.displayCount * 3);
    
            geometry.addAttribute("position", new THREE.BufferAttribute(positions, 3));
    
            // material
            const material = new THREE.LineBasicMaterial({
                color: color,
                linewidth: 2
            });
    
            // line
            const line      = new THREE.Line(geometry, material);
            line.name       = name;
            line.position.x = this.props.rangeX / -2;
            this.scene!.add(line);
    
            const chartState: ChartState = {
                array: positions,
                mesh: line,
                count: 0,
                index: 0,
            };
    
            // copyArray.push(chartState);
            // setChartStateList(copyArray);
    
            this.chartStateList.push(chartState);
        }
    }
    
    private animate = () => {
        // console.log('animate');
        // trackball?.update();
        
        // stats.update();
        requestAnimationFrame(this.animate);
        this.renderer!.render(this.scene!, this.camera!);
    }
    
    private shift = () => {
        // const state = chartStateList.find(state => state.mesh.name == name);
        // if (state != null) {
    
        // }
        for (let j= 0; j < this.chartStateList.length; j++) {
            const pos = this.chartStateList[j].array;
    
            for (let i = 0; i < pos.length - 3; i++) {
                if (i % 3 == 0) continue;
                pos[i] = pos[i + 3];
            }
        }
    }
    
    private updateChart = () => {

        const copy = JSON.parse(JSON.stringify(this.props.watchList)) as PlotData[];
    
        for (let i = 0; i < this.chartStateList.length; i++) {
            const mesh      = this.chartStateList[i].mesh;
            const pos       = this.chartStateList[i].array;
            const lineCount = this.chartStateList[i].count;
            const lineIndex = this.chartStateList[i].index;
    
            // console.log(mesh.name, watchList[i].data);
            
            if (lineCount >= this.props.displayCount) {
                this.shift();
                pos[lineCount * 3 - 3] = lineIndex;
                // pos[lineCount * 3 - 2] = watchList[i].data;
                pos[lineCount * 3 - 2] = copy[i].data;
                pos[lineCount * 3 - 1] = 0;
                // console.log(mesh.name, watchList[i].data);
            } else {
                pos[lineCount * 3 + 0] = lineIndex;
                // pos[lineCount * 3 + 1] = watchList[i].data;
                pos[lineCount * 3 - 2] = copy[i].data;
                pos[lineCount * 3 + 2] = 0;
                this.chartStateList[i].count++;
                mesh.geometry.setDrawRange(0, this.chartStateList[i].count);
                this.chartStateList[i].index += this.det;
                // console.log(mesh.name, this.chartStateList[i].index, this.chartStateList[i].count);
            }
            mesh.geometry.attributes.position.needsUpdate = true;
          }
    };
    
    private updateChart2 = (v: number, i: number) => {
        
        if (this.chartStateList.length <= i) return;
    
        console.log("update2");
        console.log('after: ', v);
        const mesh      = this.chartStateList[i].mesh;
        const pos       = this.chartStateList[i].array;
        const lineCount = this.chartStateList[i].count;
        const lineIndex = this.chartStateList[i].index;
    
    
        // console.log(mesh.name, watchList[i].data);
        
        if (lineCount >= this.props.displayCount) {
            this.shift();
            pos[lineCount * 3 - 3] = lineIndex;
            // pos[lineCount * 3 - 2] = watchList[i].data;
            pos[lineCount * 3 - 2] = v;
            pos[lineCount * 3 - 1] = 0;
            // console.log(mesh.name, watchList[i].data);
        } else {
            pos[lineCount * 3 + 0] = lineIndex;
            // pos[lineCount * 3 + 1] = watchList[i].data;
            pos[lineCount * 3 - 2] = v;
            pos[lineCount * 3 + 2] = 0;
            this.chartStateList[i].count++;
            mesh.geometry.setDrawRange(0, this.chartStateList[i].count);
            this.chartStateList[i].index += this.det;
            // console.log(mesh.name, this.chartStateList[i].index, this.chartStateList[i].count);
        }
        mesh.geometry.attributes.position.needsUpdate = true;
    };

    componentDidMount() {
        this.init();

        this.loopId = window.setInterval(() => {
            this.updateChart();
        }, 1); 
    }

    componentWillUnmount() {
        clearInterval(this.loopId);
    }

    render() {

        return (
        
            <canvas id={this.props.canvasId} className="w-full h-full">
                <div className="text-white">sensor 1 force</div>
            </canvas>
        );
    }
}


// export default function RealTimeChartThree({canvasId, watchList, rangeY, rangeX=100, displayCount=100}: RealTimeChartThreeProps) {

//     // const [chartStateList, setChartStateList] = useState<ChartState[]>([]);
//     // const [scene, setScene] = useState<THREE.Scene|null>(null);
//     // const [camera, setCamera] = useState<THREE.PerspectiveCamera|null>(null);
//     // const [renderer, setRenderer] = useState<THREE.WebGLRenderer|null>(null);
//     // const [axisX, setAxisX] = useState<THREE.ArrowHelper|null>(null);
//     // const [axisY, setAxisY] = useState<THREE.ArrowHelper|null>(null);
//     // const [det, setDet] = useState<number>(0);
//     const [magic, setMagic] = useState<number>(0);

//     const chartStateList: ChartState[] = []
//     let scene: THREE.Scene|null = null;
//     let camera: THREE.PerspectiveCamera|null = null;
//     let renderer: THREE.WebGLRenderer| null = null;
//     let axisX: THREE.ArrowHelper| null = null;
//     let axisY: THREE.ArrowHelper| null = null;
//     let det: number = 0;
//     // let trackball: TrackballControls| null = null;

//     // const stats: Stats = new Stats();

//     const values: number[] = JSON.parse(JSON.stringify(watchList)).map(v => 0);

//     const setArray = (v:number, i: number) => {
//         // console.log("set " + i + ": " + v);
//         setMagic(v);
//         values[i] = v;
//     }
    
//     const getArray = (i: number) => {
//         console.log("get + i" + ": " + values[i]);
//         return values[i];
//     }
    
//     useEffect(() => {
//         const copy = JSON.parse(JSON.stringify(watchList));
//         for (let i= 0; i< copy.length;i++) { 
//             updateChart2(copy[i].data, i);
//             // getArray(i);
//         }
        
//         // console.log(values[0]);
//     }, [watchList])

//     const init = (getFunc: (i: number)=> number) => {

//         const canvas = document.getElementById(canvasId) as HTMLCanvasElement;

//         if (canvas != null) {

//             // setScene(new THREE.Scene());
//             scene = new THREE.Scene();

//             // setCamera(new THREE.PerspectiveCamera(
//             //     75,
//             //     canvas.clientWidth / canvas.clientHeight,
//             //     0.1,
//             //     1000)
//             // );
//             camera = new THREE.PerspectiveCamera(
//                 75,
//                 canvas.clientWidth / canvas.clientHeight,
//                 0.1,
//                 1000
//             );

//             // setRenderer(new THREE.WebGLRenderer({
//             //     canvas: canvas
//             // }));

//             renderer = new THREE.WebGLRenderer({
//                 canvas: canvas
//             });


//             renderer?.setPixelRatio(window.devicePixelRatio);
//             renderer?.setSize(canvas.clientWidth, canvas.clientHeight);

//             // trackball = new TrackballControls(camera, renderer.domElement);

//             // trackball.rotateSpeed = 5.0; //回転速度
//             // trackball.noRoll = true;
//             // trackball.zoomSpeed = 0.5; //ズーム速度
//             // trackball.panSpeed = 0.5; //パン速度

//             // setAxisX(new THREE.ArrowHelper(
//             //     new THREE.Vector3(1, 0, 0),
//             //     new THREE.Vector3(rangeX / -2, 0, 0),
//             //     rangeX,
//             //     "#ff0000",
//             //     0.2,
//             //     0.2
//             //   )
//             // );
//             axisX = new THREE.ArrowHelper(
//                 new THREE.Vector3(1, 0, 0),
//                 new THREE.Vector3(rangeX / -2, 0, 0),
//                 rangeX,
//                 "#ff0000",
//                 0.2,
//                 0.2
//               )


//             // setAxisY(new THREE.ArrowHelper(
//             //     new THREE.Vector3(0, 1, 0),
//             //     new THREE.Vector3(rangeX / -2, -rangeY, 0),
//             //     rangeY * 2,
//             //     "#00ff00",
//             //     0.2,
//             //     0.2)
//             //     );
//             axisY = new THREE.ArrowHelper(
//                 new THREE.Vector3(0, 1, 0),
//                 new THREE.Vector3(rangeX / -2, rangeY / -2, 0),
//                 rangeY,
//                 "#00ff00",
//                 0.2,
//                 0.2
//             );


//             if(scene != null) {
//                 if (axisX != null) scene.add(axisX);
//                 if (axisY != null) scene.add(axisY);
//             }


//             // setDet(rangeX / displayCount);
//             det = rangeX / displayCount;
//             if (camera != null) {
//                 camera.position.z = rangeX * 1.0;
//             }

//             const colorList = [
//                 "red",
//                 "green",
//                 "blue",
//             ];


//             for (let i= 0; i < watchList.length; i++) {
//                 addChart(colorList[i % colorList.length], watchList[i].name);
//                 console.log(watchList[i].name, " add.");
//             }

//             // window.setInterval(() => {
//             //     // setArray(Math.random(), 0);
//             //     // console.log("update: ", getArray(0));
//             //     console.log("update: ", getFunc(0));
//             //     updateChart();
//             // }, 10);

//             // stats.showPanel(0);
//             // div!.appendChild(stats.dom);

//             animate();
//         }


//     }

//     const addChart = (color: string|number|THREE.Color, name: string) => {

//         if (scene != null) {

//             // const copyArray = JSON.parse(JSON.stringify(chartStateList));

//             const geometry  = new THREE.BufferGeometry();
//             const positions = new Float32Array(displayCount * 3);

//             geometry.addAttribute("position", new THREE.BufferAttribute(positions, 3));

//             // material
//             const material = new THREE.LineBasicMaterial({
//                 color: color,
//                 linewidth: 2
//             });

//             // line
//             const line      = new THREE.Line(geometry, material);
//             line.name       = name;
//             line.position.x = rangeX / -2;
//             scene.add(line);

//             const chartState: ChartState = {
//                 array: positions,
//                 mesh: line,
//                 count: 0,
//                 index: 0,
//             };

//             // copyArray.push(chartState);
//             // setChartStateList(copyArray);

//             chartStateList.push(chartState);
//         }
//     }

//     const animate = () => {
//         // console.log('animate');
//         // trackball?.update();
        
//         // stats.update();
//         requestAnimationFrame(animate);
//         renderer!.render(scene!, camera!);
//     }

//     const shift = () => {
//         // const state = chartStateList.find(state => state.mesh.name == name);
//         // if (state != null) {

//         // }
//         for (let j= 0; j < chartStateList.length; j++) {
//             const pos = chartStateList[j].array;

//             for (let i = 0; i < pos.length - 3; i++) {
//                 if (i % 3 == 0) continue;
//                 pos[i] = pos[i + 3];
//             }
//         }
//     }

//     const updateChart = () => {

//         if (values.length <= 0) return;
//         for (let i = 0; i < chartStateList.length; i++) {
//             const mesh      = chartStateList[i].mesh;
//             const pos       = chartStateList[i].array;
//             const lineCount = chartStateList[i].count;
//             const lineIndex = chartStateList[i].index;

//             console.log('after: ', values[0]);

//             // console.log(mesh.name, watchList[i].data);
            
//             if (lineCount >= displayCount) {
//                 shift();
//                 pos[lineCount * 3 - 3] = lineIndex;
//                 // pos[lineCount * 3 - 2] = watchList[i].data;
//                 pos[lineCount * 3 - 2] = getArray(i);
//                 pos[lineCount * 3 - 1] = 0;
//                 // console.log(mesh.name, watchList[i].data);
//             } else {
//                 pos[lineCount * 3 + 0] = lineIndex;
//                 // pos[lineCount * 3 + 1] = watchList[i].data;
//                 pos[lineCount * 3 - 2] = getArray(i);
//                 pos[lineCount * 3 + 2] = 0;
//                 chartStateList[i].count++;
//                 mesh.geometry.setDrawRange(0, chartStateList[i].count);
//                 chartStateList[i].index += det;
//                 // console.log(mesh.name, chartStateList[i].index, chartStateList[i].count);
//             }
//             mesh.geometry.attributes.position.needsUpdate = true;
//           }
//     };

//     const updateChart2 = (v: number, i: number) => {
        
//         if (chartStateList.length <= i) return;

//         console.log("update2");
//         console.log('after: ', v);
//         const mesh      = chartStateList[i].mesh;
//         const pos       = chartStateList[i].array;
//         const lineCount = chartStateList[i].count;
//         const lineIndex = chartStateList[i].index;


//         // console.log(mesh.name, watchList[i].data);
        
//         if (lineCount >= displayCount) {
//             shift();
//             pos[lineCount * 3 - 3] = lineIndex;
//             // pos[lineCount * 3 - 2] = watchList[i].data;
//             pos[lineCount * 3 - 2] = v;
//             pos[lineCount * 3 - 1] = 0;
//             // console.log(mesh.name, watchList[i].data);
//         } else {
//             pos[lineCount * 3 + 0] = lineIndex;
//             // pos[lineCount * 3 + 1] = watchList[i].data;
//             pos[lineCount * 3 - 2] = v;
//             pos[lineCount * 3 + 2] = 0;
//             chartStateList[i].count++;
//             mesh.geometry.setDrawRange(0, chartStateList[i].count);
//             chartStateList[i].index += det;
//             // console.log(mesh.name, chartStateList[i].index, chartStateList[i].count);
//         }
//         mesh.geometry.attributes.position.needsUpdate = true;
//     };

//     useEffect(() => {
//         init(getArray);
//     }, []);

//     return (

//         <canvas id={canvasId} className="w-full h-full">
//             <div className="text-white">sensor 1 force</div>
//         </canvas>
//     );
// }
