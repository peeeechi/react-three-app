import React from 'react';
import Layout from '@/components/Layout'

const IndexPage: React.FunctionComponent = () => {
  return (
    <>
      <header className="container mx-auto">TailwindCss 練習</header>

      <h1 className="text-4xl text-green-700 text-center font-semibold">hello tailwind css</h1>

      <hr/>

      <div className="container flex justify-center mx-auto">

        <div className="text-center">
          <h2 className="text-3xl font-semibold m-2">font size</h2>
            <p className="text-xs">text-xs</p>
            <p className="text-sm">text-sm</p>
            <p className="text-base">text-base</p>
            <p className="text-lg">text-lg</p>
            <p className="text-xl">text-xl</p>
            <p className="text-2xl">text-2xl</p>
            <p className="text-3xl">text-3xl</p>
            <p className="text-4xl">text-4xl</p>
            <p className="text-5xl">text-5xl</p>
            <p className="text-6xl">text-6xl</p>
        </div>

        <div className="text-center">
          <h2 className="text-3xl font-semibold m-2">font bold</h2>
          <p className="font-hairline">font-hairline</p>
          <p className="font-thin">font-thin</p>
          <p className="font-light">font-light</p>
          <p className="font-normal">font-normal</p>
          <p className="font-medium">font-medium</p>
          <p className="font-semibold">font-semibold</p>
          <p className="font-bold">font-bold</p>
          <p className="font-extrabold">font-extrabold</p>
          <p className="font-black">font-black</p>
        </div>
      </div>

      <hr/>


      <hr/>

      <div className="text-center">
        <h2 className="text-3xl font-semibold">Button</h2>
        <button className="px-4 py-2 bg-blue-500  mx-2 mb-2 block">Button</button>
        <button className="px-4 py-2 bg-blue-500 m-2 rounded block">Button rounded</button>
        <button className="px-4 py-2 bg-blue-500 m-2 rounded-full block">Button rounded-full</button>
        <button className="px-4 py-2 bg-blue-500 m-2 rounded-full block focus:outline-none">Button rounded-full focus:outline-none</button>
      </div>

    </>
  )
}

export default IndexPage
