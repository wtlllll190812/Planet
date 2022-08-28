require("@nomiclabs/hardhat-waffle");

describe("Planet", function() {
    it("URI", async function() {
        const [owner, addr1] = await ethers.getSigners();

        const Planet = await ethers.getContractFactory("Planet");
        const planet = await Planet.deploy("Hello World!");
        await planet.deployed();

        /// Test MintNFT
        await planet.connect(owner).MintNFT(
            ["1", "2"], [1, 100]
        );
        // await planet.connect(owner).MintNFT(
        //     ["1", "2"], [0, 100]
        // );
        // await planet.connect(addr1).MintNFT(
        //     ["1", "2"], [1, 100]
        // );
        // console.log(await planet.GetNFT(owner.address));

        /// Test TransferNFT
        await planet.connect(owner).TransferNFT(
            owner.address, addr1.address, 1, 1
        );
        await planet.connect(owner).TransferNFT(
            owner.address, addr1.address, 2, 50
        );
        // await planet.connect(addr1).TransferNFT(
        //     owner.address, addr1.address, 2, 50
        // );
        // await planet.connect(owner).TransferNFT(
        //     owner.address, addr1.address, 1, 0
        // );
        // await planet.connect(owner).TransferNFT(
        //     owner.address, addr1.address, 1, 100
        // );
        // console.log(await planet.GetNFT(owner.address));
        // console.log(await planet.GetNFT(addr1.address));

        /// Test AddItemToMarket
        // console.log(await planet.GetUnsoldItems());
        await planet.connect(owner).AddItemToMarket(
            2, 30, 100
        );
        // console.log(await planet.GetNFT(owner.address));
        // console.log(await planet.GetUnsoldItems());
        await planet.connect(owner).AddItemToMarket(
            2, 20, 200
        );
        // console.log(await planet.GetNFT(owner.address));
        // console.log(await planet.GetUnsoldItems());
        // await planet.connect(owner).AddItemToMarket(
        //     2, 0, 200
        // );
        // await planet.connect(owner).AddItemToMarket(
        //     2, 100, 200
        // );
        // await planet.connect(owner).AddItemToMarket(
        //     3, 10, 200
        // );

        /// Test RedeemItems
        await planet.connect(owner).RedeemItems(
            1, 30
        );
        // await planet.connect(owner).RedeemItems(
        //     2, 30
        // );
        // await planet.connect(addr1).RedeemItems(
        //     2, 20
        // );
        // await planet.connect(owner).RedeemItems(
        //     3, 30
        // );
        // await planet.connect(owner).RedeemItems(
        //     2, 0
        // );
        // console.log(await planet.GetNFT(owner.address));
        // console.log(await planet.GetUnsoldItems());

        /// Test BuyItemAndTransferOwnership
        console.log("owner:", await owner.provider.getBalance(owner.address));
        console.log("addr1:", await owner.provider.getBalance(addr1.address));
        let overrides = {value: 4000};
        await planet.connect(addr1).BuyItemAndTransferOwnership(
            2, 20, overrides
        );
        console.log(await planet.GetNFT(owner.address));
        console.log(await planet.GetNFT(addr1.address));
        console.log(await planet.GetUnsoldItems());
        console.log("owner:", await owner.provider.getBalance(owner.address));
        console.log("addr1:", await owner.provider.getBalance(addr1.address));
    });
})