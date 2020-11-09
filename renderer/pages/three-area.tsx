import React from 'react';
import Layout from '@/components/Layout'
import {THREE_AREA_ID} from '@/utils/constant';
import {threeApi} from '@/utils/instance-store';

class ThreeAreaPage extends React.Component {

    componentDidMount() {
        console.log("three:", threeApi);
        threeApi.init(1400, 700);
    }

    render() {
        return (
            <Layout title="Three Area | Next.js + TypeScript + Electron Example">
                <div>
                    <canvas id={THREE_AREA_ID}></canvas>
                </div>
            </Layout>
        )
    }
}

export default ThreeAreaPage;