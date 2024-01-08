const MeetingRoom = artifacts.require("MeetingRoom");

module.exports = function (deployer) {
  deployer.deploy(MeetingRoom);
};