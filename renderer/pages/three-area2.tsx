import React from 'react';
import {THREE_AREA_ID_2} from '@/utils/constant';
import * as three from 'three';
import WebSocketClient from '@/utils/websocket-client';
import ThreeApi, {loadGLTF, deg2rad, changeMaterial} from '@/utils/three-api';
import {WS_WEBSOCKET_CONTROLLER_URL_2, WS_WEBSOCKET_CONTROLLER_TEST_URL_2} from '@/utils/constant';
import NormalButton from '@/components/Button';
import ListTable from '@/components/ListTable';
// import PlotArea from '@/components/PlotArea';
// import RealtimeChart from '@/components/RealtimeChartPlotly';
import RealtimeChart from '@/components/RealTimeChartThree';
import "@/interfaces";
import { ForceSensorResponseDouble, ForceSensorData } from '@/interfaces/force-sensor-response';

const threeApi = new ThreeApi(THREE_AREA_ID_2, new three.Color(0x111111));

const test = true;

const URL = test? WS_WEBSOCKET_CONTROLLER_TEST_URL_2 : WS_WEBSOCKET_CONTROLLER_URL_2;

const plotMaxLength = 1000;

type ForceData = {
    "fx": number[],
    "fy": number[],
    "fz": number[],
}

type MomentData = {
    "mx": number[],
    "my": number[],
    "mz": number[],
}

type ThreeAreaState = {
    forceSensorResponse: ForceSensorResponseDouble,
    sensor1PlotDataForce: ForceData,
    sensor1PlotDataMoment: MomentData,
    sensor2PlotDataForce: ForceData,
    sensor2PlotDataMoment: MomentData,
    logCount: number,
    isMoment: boolean,
};

class ThreeAreaPage extends React.Component<any, ThreeAreaState>{

    public forceSensorClient: WebSocketClient|null = null;
    private connectionKeepLoopId: number|null = null;

    private logBuffer: ForceSensorResponseDouble[]    = [];
    private isLogging: boolean                  = false;

    private sensor1ForceArrow: three.ArrowHelper = new three.ArrowHelper(
        new three.Vector3(0,0,1), /* dir */
        new three.Vector3(0,0,0), /* origin */
        0, /* length */
        0xffff55, /* color */
        0.2,
        0.2
    );

    private sensor2ForceArrow: three.ArrowHelper = new three.ArrowHelper(
        new three.Vector3(0,0,1), /* dir */
        new three.Vector3(0,0,0), /* origin */
        0, /* length */
        0x55ffff, /* color */
        0.2,
        0.2
    );

    private combindForceArrow: three.ArrowHelper = new three.ArrowHelper(
        new three.Vector3(0,0,1), /* dir */
        new three.Vector3(0,0,0), /* origin */
        0, /* length */
        0xff55ff, /* color */
        0.2,
        0.2
    );

    constructor(props: any) {
        super(props);

        this.state = {
            forceSensorResponse: {
                "sensor-1": {fx:0,fy:0,fz:0,mx:0,my:0,mz:0} as ForceSensorData,
                "sensor-2": {fx:0,fy:0,fz:0,mx:0,my:0,mz:0} as ForceSensorData,
                "combined": {fx:0,fy:0,fz:0,mx:0,my:0,mz:0} as ForceSensorData,
                "time-stamp": 0,
            },
            logCount: 0,
            isMoment: false,
            sensor1PlotDataForce: {
                fx:[],
                fy:[],
                fz:[],
            },
            sensor1PlotDataMoment: {
                mx: [],
                my: [],
                mz: [],
            },
            sensor2PlotDataForce: {
                fx:[],
                fy:[],
                fz:[],
            },
            sensor2PlotDataMoment: {
                mx: [],
                my: [],
                mz: [],
            }
        };
    }


    private websocketConnectionKeep = () => {
        if(this.forceSensorClient == null)
            return;

        switch (this.forceSensorClient.websocketState) {
            case "close":
            case "error":
                this.forceSensorClient.connect(URL);
                break;
            case "connecting":
            default:
                // 何もしない
                break;
        }
    }


    /**
     * コンポーネントがマウントされた時の処理
     */
    componentDidMount() {
        console.log("did mounted");

        this.forceSensorClient = new WebSocketClient();
        this.forceSensorClient.ConnectionStateChangedEvent.push((state) => {
            // alert(state);
        });

        this.forceSensorClient.MessageReceivedEvent.push((event) => {
            const resJson = event.data;
            const res = JSON.parse(resJson) as ForceSensorResponseDouble;
            // console.log(res);
            this.setState({forceSensorResponse: res});
            this.test(res);
        });

        this.connectionKeepLoopId = window.setInterval(this.websocketConnectionKeep, 1000);

        threeApi.init();

        const addPLight = (x:number, y:number, z:number) => {

            const pLight        = new three.PointLight("#ffffff", 1);
            pLight.castShadow   = true;
            pLight.position.set(x, y, z);

            threeApi.addObject(pLight);
        };

        const lightLength = 50;

        addPLight(0,0,lightLength);
        addPLight(0,0,-lightLength);
        addPLight(lightLength,lightLength,0);
        addPLight(-lightLength,lightLength,0);
        addPLight(lightLength,-lightLength,0);
        addPLight(-lightLength,-lightLength,0);

        threeApi.addObject(this.sensor1ForceArrow);
        loadGLTF("minebea-force-sensor.glb").then(gltf => {
        // loadGLTF("test.glb").then(gltf => {

            changeMaterial(gltf.scene.children, 0.3);

            const sensor1Group = gltf.scene;
            const sensor2Group = sensor1Group.clone();

            sensor1Group.position.z -= 1.5;
            sensor2Group.position.z += 1.5;

            sensor2Group.rotateY(deg2rad(180));

            threeApi.addObject(this.sensor1ForceArrow);
            threeApi.addObject(this.sensor2ForceArrow);
            threeApi.addObject(this.combindForceArrow);

            threeApi.addObject(sensor1Group);
            threeApi.addObject(sensor2Group);

            threeApi.setTrackball(threeApi.scene.position);
        }).catch(error => {
            console.error('gltf load error: ', error);
        });
    }

    componentWillUnmount() {
        if(this.connectionKeepLoopId != null) {
            window.clearInterval(this.connectionKeepLoopId);
            this.connectionKeepLoopId = null;
        }
        threeApi.disposeOrbit();
        threeApi.disposeTrackball();
        
        if (this.forceSensorClient != null ) {
            if (this.forceSensorClient.websocketState == "connecting") {
                this.forceSensorClient.Disconnect();
            }
            this.forceSensorClient = null;
        }
    }

    recToggle = () => {
        if(!this.isLogging) this.dataClear();

        this.isLogging = !this.isLogging;
    }

    dataClear = () => {
        this.logBuffer = [];
        this.setState({logCount: 0});
    }

    saveData = () => {

        if (this.isLogging) {
            alert("データロギング中は保存できません");
            return;
        }

        if (this.logBuffer.length > 0) {
            const rows = ["s1-fx,s1-fy,s1-fz,s1-mx,s1-my,s1-mz,s2-fx,s2-fy,s2-fz,s2-mx,s2-my,s2-mz,combind-fx,combind-fy,combind-fz,combind-mx,combind-my,combind-mz"];
            this.logBuffer.forEach(log => {
                const dataBuf = [
                    log["sensor-1"]?.fx,
                    log["sensor-1"]?.fy,
                    log["sensor-1"]?.fz,
                    log["sensor-1"]?.mx,
                    log["sensor-1"]?.my,
                    log["sensor-1"]?.mz,
                    log["sensor-2"]?.fx,
                    log["sensor-2"]?.fy,
                    log["sensor-2"]?.fz,
                    log["sensor-2"]?.mx,
                    log["sensor-2"]?.my,
                    log["sensor-2"]?.mz,
                    log["combined"]?.fx,
                    log["combined"]?.fy,
                    log["combined"]?.fz,
                    log["combined"]?.mx,
                    log["combined"]?.my,
                    log["combined"]?.mz,
                ];
                rows.push(dataBuf.join(","));
            });

            const csvStr = rows.join("\r\n");

            this.saveAndDounload(csvStr, "test-data.csv");

            this.dataClear();
        }
    }
    saveAndDounload = (data: string, fileName: string) => {
        // const bom = new Uint8Array([0xEF, 0xBB, 0xBF]);
        // const blob = new Blob([ bom, content ], { "type" : "text/csv" });

        const blob = new Blob([data], { "type" : "text/csv" });

        if (window.navigator.msSaveBlob) {
            window.navigator.msSaveBlob(blob, fileName);

            // msSaveOrOpenBlobの場合はファイルを保存せずに開ける
            window.navigator.msSaveOrOpenBlob(blob, fileName);
        } else {

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            document.body.appendChild(a);
            a.download = fileName;
            a.href = url;
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        }

    }

    test(response: ForceSensorResponseDouble) {

        if (this.isLogging) {
            this.logBuffer.push(response);
            this.setState({logCount: this.logBuffer.length});
        }

        const setArrow = (data: ForceSensorData | null, target: three.ArrowHelper) => {
            const dir = new three.Vector3(0,0,0);
            if (data != null) {
                const pos = this.state.isMoment? new three.Vector3(data.mx*100, data.my*100, data.mz*100) :new three.Vector3(data.fx, data.fy, data.fz) ;
                dir.x = pos.x;
                dir.y = pos.y;
                dir.z = pos.z;
            }


            target.setDirection(dir);
            target.setLength(dir.length() * 10);
        }


        if (response["sensor-1"] != null) {
            const data = response["sensor-1"];
            const srcForce  = Object.assign({}, this.state.sensor1PlotDataForce);
            const srcMoment = this.state.sensor1PlotDataMoment;
            
            srcForce.fx.push(data.fx);
            if (srcForce.fx.length > plotMaxLength) srcForce.fx.shift();
            srcForce.fy.push(data.fy);
            if (srcForce.fy.length > plotMaxLength) srcForce.fy.shift();
            srcForce.fz.push(data.fz);
            if (srcForce.fz.length > plotMaxLength) srcForce.fz.shift();

            srcMoment.mx.push(data.mx);
            if (srcMoment.mx.length > plotMaxLength) srcMoment.mx.shift();
            srcMoment.my.push(data.my);
            if (srcMoment.my.length > plotMaxLength) srcMoment.my.shift();
            srcMoment.mz.push(data.mz);
            if (srcMoment.mz.length > plotMaxLength) srcMoment.mz.shift();
            
            this.setState({
                sensor1PlotDataForce: srcForce,
                sensor1PlotDataMoment: srcMoment,
            });
        }
        
        if (response["sensor-2"] != null) {
            const data = response["sensor-2"];
            const srcForce  = Object.assign({}, this.state.sensor2PlotDataForce);
            const srcMoment = Object.assign({}, this.state.sensor2PlotDataMoment);
            
            srcForce.fx.push(data.fx);
            if (srcForce.fx.length > plotMaxLength) srcForce.fx.shift();
            srcForce.fy.push(data.fy);
            if (srcForce.fy.length > plotMaxLength) srcForce.fy.shift();
            srcForce.fz.push(data.fz);
            if (srcForce.fz.length > plotMaxLength) srcForce.fz.shift();

            srcMoment.mx.push(data.mx);
            if (srcMoment.mx.length > plotMaxLength) srcMoment.mx.shift();
            srcMoment.my.push(data.my);
            if (srcMoment.my.length > plotMaxLength) srcMoment.my.shift();
            srcMoment.mz.push(data.mz);
            if (srcMoment.mz.length > plotMaxLength) srcMoment.mz.shift();
            
            this.setState({
                sensor2PlotDataForce: srcForce,
                sensor2PlotDataMoment: srcMoment,
            });
        }
        
        // console.log('sensor1 force length:', this.state.sensor1PlotDataForce.fx.length);

        setArrow(response["sensor-1"], this.sensor1ForceArrow);
        setArrow(response["sensor-2"], this.sensor2ForceArrow);
        setArrow(response["combined"], this.combindForceArrow);

    }

    toggleChange = () => {
        this.setState({isMoment: !this.state.isMoment});
    }

    render() {
        return (
            <>
                <div className="grid grid-rows-3 grid-cols-12 w-full h-full">

                            <div className="row-span-2 col-span-6 bg-gray-200">
                                <canvas className="w-full h-full" id={THREE_AREA_ID_2}></canvas>
                            </div>

                            <div className="row-span-2 col-span-6 bg-green-100">
                                <div className="grid grid-rows-2 grid-cols-2 w-full h-full">
                                    <RealtimeChart canvasId="plot1" displayCount={1000} rangeX={10} rangeY={10} watchList={
                                        [
                                            {data: this.state.forceSensorResponse["sensor-1"]!.fx, name: "sensor1-fx"},
                                            {data: this.state.forceSensorResponse["sensor-1"]!.fy, name: "sensor1-fy"},
                                            {data: this.state.forceSensorResponse["sensor-1"]!.fz, name: "sensor1-fz"},
                                        ]
                                    }></RealtimeChart>
                                    <RealtimeChart canvasId="plot2" displayCount={1000} rangeX={10} rangeY={10} watchList={
                                        [
                                            {data: this.state.forceSensorResponse["sensor-1"]!.mx, name: "sensor1-mx"},
                                            {data: this.state.forceSensorResponse["sensor-1"]!.my, name: "sensor1-my"},
                                            {data: this.state.forceSensorResponse["sensor-1"]!.mz, name: "sensor1-mz"},
                                        ]
                                    }></RealtimeChart>
                                    <RealtimeChart canvasId="plot3" displayCount={1000} rangeX={10} rangeY={10} watchList={
                                        [
                                            {data: this.state.forceSensorResponse["sensor-2"]!.fx, name: "sensor2-fx"},
                                            {data: this.state.forceSensorResponse["sensor-2"]!.fy, name: "sensor2-fy"},
                                            {data: this.state.forceSensorResponse["sensor-2"]!.fz, name: "sensor2-fz"},
                                        ]
                                    }></RealtimeChart>
                                    <RealtimeChart canvasId="plot4" displayCount={1000} rangeX={10} rangeY={10} watchList={
                                        [
                                            {data: this.state.forceSensorResponse["sensor-2"]!.fx, name: "sensor2-mx"},
                                            {data: this.state.forceSensorResponse["sensor-2"]!.fy, name: "sensor2-my"},
                                            {data: this.state.forceSensorResponse["sensor-2"]!.fz, name: "sensor2-mz"},
                                        ]
                                    }></RealtimeChart>
                                    {/* <RealtimeChart length={this.state.sensor1PlotDataForce.fx.length} plotId="plot2"></RealtimeChart> */}
                                    {/* <RealtimeChart length={this.state.sensor1PlotDataForce.fx.length} plotId="plot3"></RealtimeChart> */}
                                    {/* <RealtimeChart length={this.state.sensor1PlotDataForce.fx.length} plotId="plot4"></RealtimeChart> */}

                                    {/* <div className="bg-pink-600"></div> */}
                                    {/* <div className="bg-green-600"></div> */}
                                    {/* <div className="bg-blue-600"></div> */}
                                    {/* <div className="bg-red-600"></div> */}

                                </div>
                            </div>


                            <div className="col-span-2">
                                <h3>sensor 1</h3>
                                <ListTable obj={this.state.forceSensorResponse["sensor-1"]}/>
                            </div>

                            <div className="col-span-2">
                                <h3>sensor 2</h3>
                                <ListTable obj={this.state.forceSensorResponse["sensor-2"]}/>
                            </div>

                            <div className="col-span-2">
                                <div className="flex flex-col justify-center">

                                    <p>{`log count: ${this.state.logCount}`}</p>
                                    <NormalButton className="m-2" colorType="blue" onClick={this.toggleChange}>{this.state.isMoment? "Show force" : "Show moment"}</NormalButton>
                                    <NormalButton className="m-2" colorType="blue" onClick={this.recToggle}>{this.isLogging? "Stop": "Start"}</NormalButton>
                                    <NormalButton className="m-2" colorType="blue" onClick={this.saveData}>Save to csv file</NormalButton>
                                </div>
                            </div>

                </div>
            </>
        );
    }
}

export default ThreeAreaPage;