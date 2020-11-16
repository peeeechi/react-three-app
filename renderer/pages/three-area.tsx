import React from 'react';
import {THREE_AREA_ID} from '@/utils/constant';
import * as three from 'three';
import {threeApi} from '@/utils/instance-store';
import WebSocketClient from '@/utils/websocket-client';
import {loadGLTF} from '@/utils/three-api';
import {WS_WEBSOCKET_CONTROLLER_TEST_URL} from '@/utils/constant';
import "@/interfaces";
import { ForceSensorResponse } from '@/interfaces/force-sensor-response';

let forceSensorClient: WebSocketClient|null = null;

type ThreeAreaState = {
    forceSensorResponse: ForceSensorResponse,
};

class ThreeAreaPage extends React.Component<any, ThreeAreaState>{

    constructor(props: any) {
        super(props);

        this.state = {
            forceSensorResponse: {
                "sensor-1": null,
                "sensor-2": null,
                "time-stamp": 0,
            },
        };
    }

    private sensor1obj: three.ArrowHelper = new three.ArrowHelper(
        new three.Vector3(0,0,1), /* dir */
        new three.Vector3(0,0,0), /* origin */
        0, /* length */
        0xffffff, /* color */
        0.2,
        0.2
    );

    componentDidMount() {
        console.log("did mounted");

        forceSensorClient = new WebSocketClient();
        forceSensorClient.connect(WS_WEBSOCKET_CONTROLLER_TEST_URL);
        forceSensorClient.ConnectionStateChangedEvent.push((state) => {
            alert(state);
        });

        forceSensorClient.MessageReceivedEvent.push((event) => {
            const resJson = event.data;
            const res = JSON.parse(resJson) as ForceSensorResponse;
            console.log(res);
            this.setState({forceSensorResponse: res});
            this.test(res);
        });


        threeApi.init();
        threeApi.addObject(this.sensor1obj);
        loadGLTF("minebea-force-sensor.glb").then(gltf => {
            threeApi.addObject(gltf.scene);
            threeApi.setTrackball(gltf.scene.position);
        }).catch(error => {
            console.error('gltf load error: ', error);
        });
    }

    componentWillUnmount() {
        threeApi.disposeOrbit();
        threeApi.disposeTrackball();
    }

    test(response: ForceSensorResponse) {
        const sensor1 = response["sensor-1"];

        if (sensor1 != null) {
            const dir = new three.Vector3(sensor1.fx, sensor1.fy, sensor1.fz);
            this.sensor1obj.setDirection(dir);
            this.sensor1obj.setLength(dir.length());
        }
    }

    render() {
        return (
            <div>
                <canvas id={THREE_AREA_ID} width="1000" height="500"></canvas>

                <h3>sensor 1</h3>
                <table>
                    <thead>
                        <tr>
                            <th>item name</th><th>value</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr><td>fx</td><td>{this.state.forceSensorResponse["sensor-1"]?.fx}</td></tr>
                        <tr><td>fy</td><td>{this.state.forceSensorResponse["sensor-1"]?.fy}</td></tr>
                        <tr><td>fz</td><td>{this.state.forceSensorResponse["sensor-1"]?.fz}</td></tr>
                        <tr><td>mx</td><td>{this.state.forceSensorResponse["sensor-1"]?.mx}</td></tr>
                        <tr><td>my</td><td>{this.state.forceSensorResponse["sensor-1"]?.my}</td></tr>
                        <tr><td>mz</td><td>{this.state.forceSensorResponse["sensor-1"]?.mz}</td></tr>
                    </tbody>
                </table>
            </div>
        );
    }
}

export default ThreeAreaPage;